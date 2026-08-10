using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Model;
using AudioSeparator.FFMPEG.Entities;

namespace AudioSeparator.FFMPEG;

public class FFMPEGAudioReader(string ffmpegPath, string ffprobePath) : IAudioReader
{
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private async Task<FFProbeResult> ProbeAsync(Stream input, CancellationToken cancellationToken = default)
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
        var stderrTask = process.StandardError.ReadToEndAsync();
        var stdoutTask = process.StandardOutput.ReadToEndAsync();

        try
        {
            await input.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
        }
        catch (IOException) when (!process.HasExited)
        {
            throw;
        }
        catch (IOException) when (process.ExitCode is 0)
        {

        }

        await process.WaitForExitAsync();

        var stderr = await stderrTask;
        if (!string.IsNullOrEmpty(stderr))
        {
            throw new Exception(stderr);
        }

        var stdout = await stdoutTask;
        //Console.WriteLine(stdout);

        var result = JsonSerializer.Deserialize<FFProbeResult>(stdout, serializerOptions) ?? throw new InvalidOperationException("Invalid ffprobe json output");
        var stream = result.Streams.Single(s => s.CodecType == "audio");

        long sampleCount;
        if (stream.DurationTimeSpan.HasValue && !string.IsNullOrEmpty(stream.TimeBase) && stream.TimeBase == $"1/{stream.SampleRate}")
        {
            sampleCount = stream.DurationTimeSpan.Value;
        }
        else
        {
            sampleCount = (long)Math.Round(stream.Duration * stream.SampleRate);
        }

        // if (sampleCount is <= 0)
        // {
        //     throw new InvalidOperationException("Invalid sample count, probally ffprobe doesn't returned stream duration!");
        // }

        // Console.WriteLine(stream.DurationTimeSpan.HasValue ? stream.DurationTimeSpan.Value : -1);
        // Console.WriteLine(stream.Duration);
        // Console.WriteLine(stream.SampleRate);

        // var chunkCount = (int)Math.Ceiling(sampleCount / (double)inputSize);

        // return new AudioMetadata
        // {
        //     //SampleCount = sampleCount,
        //     //ChunkCount = chunkCount,
        //     Channels = stream.Channels,
        //     SampleRate = stream.SampleRate
        // };

        return result;
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(Stream input, int inputSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var metadata = await ProbeAsync(input, cancellationToken);
        var stream = metadata.Streams.Single(s => s.CodecType == "audio");
        var bytesPerFrame = sizeof(float) * stream.Channels;

        var chunkBytes = bytesPerFrame * inputSize;
        var buffer = ArrayPool<byte>.Shared.Rent(chunkBytes);

        input.Seek(0, SeekOrigin.Begin);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-v error -i pipe:0 -f f32le -acodec pcm_f32le -ac {stream.Channels} -ar {stream.SampleRate} pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffmpeg process");


            var copyTask = Task.Run(async () =>
            {
                await input.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
                await process.StandardInput.FlushAsync(cancellationToken);

                process.StandardInput.Close();
            });

            int chunkIndex = 0;
            while (true)
            {
                int bytesRead = 0;
                while (bytesRead < chunkBytes)
                {
                    int read = await process.StandardOutput.BaseStream.ReadAsync(buffer.AsMemory(bytesRead, chunkBytes - bytesRead), cancellationToken);
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
                var chunk = new float[framesRead * stream.Channels];

                MemoryMarshal.Cast<byte, float>(buffer.AsSpan(0, bytesRead)).CopyTo(chunk);
                yield return new AudioChunk(chunk, chunkIndex++, framesRead);

                if (framesRead < inputSize)
                {
                    yield break;
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(string fileName, int inputSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var fileStream = File.OpenRead(fileName);
        await foreach (var chunk in ReadAsync(fileStream, inputSize, cancellationToken))
        {
            yield return chunk;
        }
    }
}