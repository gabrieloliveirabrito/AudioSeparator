using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.FFMPEG.Entities;

namespace AudioSeparator.FFMPEG;

public class FFMPEGAudioReader(string ffmpegPath, string ffprobePath) : IAudioReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<AudioSourceInfo> ProbeAsync(Stream input, int inputFrameCount, CancellationToken cancellationToken = default)
    {
        var probe = await RunProbeAsync(input, cancellationToken);
        return ToSourceInfo(probe, inputFrameCount);
    }

    public async Task<AudioSourceInfo> ProbeAsync(string fileName, int inputFrameCount, CancellationToken cancellationToken = default)
    {
        var probe = await RunProbeAsync(fileName, cancellationToken);
        return ToSourceInfo(probe, inputFrameCount);
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(Stream input, int inputFrameCount, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var metadata = await RunProbeAsync(input, cancellationToken);
        var stream = metadata.Streams.Single(s => s.CodecType == "audio");

        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }

        await foreach (var chunk in DecodeFromStreamAsync(input, stream, inputFrameCount, cancellationToken))
        {
            yield return chunk;
        }
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(string fileName, int inputFrameCount, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var metadata = await RunProbeAsync(fileName, cancellationToken);
        var stream = metadata.Streams.Single(s => s.CodecType == "audio");

        await foreach (var chunk in DecodeFromFileAsync(fileName, stream, inputFrameCount, cancellationToken))
        {
            yield return chunk;
        }
    }

    private async IAsyncEnumerable<AudioChunk> DecodeFromFileAsync(
        string fileName,
        FFProbeStream stream,
        int inputFrameCount,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var bytesPerFrame = sizeof(float) * stream.Channels;
        var chunkBytes = bytesPerFrame * inputFrameCount;
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(chunkBytes);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-v error -i \"{fileName}\" -f f32le -acodec pcm_f32le -ac {stream.Channels} -ar {stream.SampleRate} pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffmpeg process");
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await foreach (var chunk in ReadDecodedChunksAsync(process, buffer, chunkBytes, bytesPerFrame, inputFrameCount, cancellationToken))
            {
                yield return chunk;
            }

            await process.WaitForExitAsync(cancellationToken);
            var stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                throw new Exception(string.IsNullOrEmpty(stderr)
                    ? $"ffmpeg exited with code {process.ExitCode}"
                    : stderr);
            }
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async IAsyncEnumerable<AudioChunk> DecodeFromStreamAsync(
        Stream input,
        FFProbeStream stream,
        int inputFrameCount,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var bytesPerFrame = sizeof(float) * stream.Channels;
        var chunkBytes = bytesPerFrame * inputFrameCount;
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(chunkBytes);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-v error -i pipe:0 -f f32le -acodec pcm_f32le -ac {stream.Channels} -ar {stream.SampleRate} pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffmpeg process");
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            _ = Task.Run(async () =>
            {
                try
                {
                    await input.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
                    await process.StandardInput.FlushAsync(cancellationToken);
                }
                catch (IOException)
                {
                    // ffmpeg may close stdin after reading enough data
                }
                finally
                {
                    process.StandardInput.Close();
                }
            }, cancellationToken);

            await foreach (var chunk in ReadDecodedChunksAsync(process, buffer, chunkBytes, bytesPerFrame, inputFrameCount, cancellationToken))
            {
                yield return chunk;
            }

            await process.WaitForExitAsync(cancellationToken);
            var stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                throw new Exception(string.IsNullOrEmpty(stderr)
                    ? $"ffmpeg exited with code {process.ExitCode}"
                    : stderr);
            }
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static async IAsyncEnumerable<AudioChunk> ReadDecodedChunksAsync(
        Process process,
        byte[] buffer,
        int chunkBytes,
        int bytesPerFrame,
        int inputFrameCount,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var chunkIndex = 0;
        while (true)
        {
            var bytesRead = 0;
            while (bytesRead < chunkBytes)
            {
                var read = await process.StandardOutput.BaseStream.ReadAsync(
                    buffer.AsMemory(bytesRead, chunkBytes - bytesRead),
                    cancellationToken);

                if (read is 0)
                {
                    break;
                }

                bytesRead += read;
            }

            if (bytesRead is 0)
            {
                yield break;
            }

            var framesRead = bytesRead / bytesPerFrame;
            var chunk = new float[framesRead * (bytesPerFrame / sizeof(float))];
            MemoryMarshal.Cast<byte, float>(buffer.AsSpan(0, bytesRead)).CopyTo(chunk);
            yield return new AudioChunk(chunk, chunkIndex++, framesRead);

            if (framesRead < inputFrameCount)
            {
                yield break;
            }
        }
    }

    private async Task<FFProbeResult> RunProbeAsync(string fileName, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffprobePath,
            Arguments = $"-v quiet -print_format json -show_format -show_streams -i \"{fileName}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffprobe process");
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stderr = await stderrTask;
        if (process.ExitCode != 0)
        {
            throw new Exception(string.IsNullOrEmpty(stderr)
                ? $"ffprobe exited with code {process.ExitCode}"
                : stderr);
        }

        var stdout = await stdoutTask;
        return JsonSerializer.Deserialize<FFProbeResult>(stdout, SerializerOptions)
            ?? throw new InvalidOperationException("Invalid ffprobe json output");
    }

    private async Task<FFProbeResult> RunProbeAsync(Stream input, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffprobePath,
            Arguments = "-v quiet -print_format json -show_format -show_streams -i pipe:0",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffprobe process");
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

        try
        {
            await input.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
        }
        catch (IOException)
        {
            // ffprobe may close stdin after reading the header
        }
        finally
        {
            process.StandardInput.Close();
        }

        await process.WaitForExitAsync(cancellationToken);

        var stderr = await stderrTask;
        if (process.ExitCode != 0)
        {
            throw new Exception(string.IsNullOrEmpty(stderr)
                ? $"ffprobe exited with code {process.ExitCode}"
                : stderr);
        }

        var stdout = await stdoutTask;
        return JsonSerializer.Deserialize<FFProbeResult>(stdout, SerializerOptions)
            ?? throw new InvalidOperationException("Invalid ffprobe json output");
    }

    private static AudioSourceInfo ToSourceInfo(FFProbeResult probe, int inputFrameCount)
    {
        var stream = probe.Streams.Single(s => s.CodecType == "audio");

        long sampleCount;
        if (stream.DurationTimeSpan.HasValue && !string.IsNullOrEmpty(stream.TimeBase) && stream.TimeBase == $"1/{stream.SampleRate}")
        {
            sampleCount = stream.DurationTimeSpan.Value;
        }
        else
        {
            sampleCount = (long)Math.Round(stream.Duration * stream.SampleRate);
        }

        var chunkCount = inputFrameCount > 0
            ? (int)Math.Ceiling(sampleCount / (double)inputFrameCount)
            : 0;

        return new AudioSourceInfo
        {
            SampleRate = stream.SampleRate,
            SampleCount = sampleCount,
            Channels = stream.Channels,
            ChunkCount = chunkCount
        };
    }
}
