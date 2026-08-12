using System.Buffers;
using System.Runtime.CompilerServices;
using AudioSeparator.Abstractions.Audio;
using NAudio.Wave;

namespace AudioSeparator.NAudio;

public class NAudioReader : IAudioReader
{
    private static void ResetStreamPosition(Stream input)
    {
        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }
    }

    private static WaveStream CreateWaveStream(Stream input)
    {
        ResetStreamPosition(input);

        try
        {
            return new StreamMediaFoundationReader(input);
        }
        catch
        {
            ResetStreamPosition(input);

            try
            {
                return new Mp3FileReader(input);
            }
            catch
            {
                ResetStreamPosition(input);
                return new WaveFileReader(input);
            }
        }
    }

    public Task<AudioSourceInfo> ProbeAsync(Stream input, int inputFrameCount, CancellationToken cancellationToken = default)
    {
        using var reader = CreateWaveStream(input);
        var waveFormat = reader.WaveFormat;
        var frames = reader.Length / waveFormat.BlockAlign;
        var chunkCount = inputFrameCount > 0
            ? (int)Math.Ceiling(frames / (double)inputFrameCount)
            : 0;

        return Task.FromResult(new AudioSourceInfo
        {
            SampleRate = waveFormat.SampleRate,
            SampleCount = frames,
            Channels = waveFormat.Channels,
            ChunkCount = chunkCount
        });
    }

    public async Task<AudioSourceInfo> ProbeAsync(string fileName, int inputFrameCount, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(fileName);
        return await ProbeAsync(stream, inputFrameCount, cancellationToken);
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(Stream input, int inputFrameCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = ArrayPool<float>.Shared.Rent(8192);
        try
        {
            using var reader = CreateWaveStream(input);
            var channels = reader.WaveFormat.Channels;
            var frames = reader.Length / reader.WaveFormat.BlockAlign;
            var chunkCount = (int)Math.Ceiling(frames / (double)inputFrameCount);
            var provider = reader.ToSampleProvider();

            var buffered = 0;
            var bufferPosition = 0;

            for (var i = 0; i < chunkCount; i++)
            {
                var chunkOffset = inputFrameCount * i;
                var chunkSize = Math.Min(inputFrameCount, (int)frames - chunkOffset);
                var chunkData = new float[chunkSize * channels];
                var sampleIndex = 0;

                while (sampleIndex < chunkSize)
                {
                    if (bufferPosition >= buffered)
                    {
                        buffered = provider.Read(buffer, 0, buffer.Length);
                        bufferPosition = 0;

                        if (buffered == 0)
                        {
                            break;
                        }
                    }

                    while (bufferPosition < buffered && sampleIndex < chunkSize)
                    {
                        var dst = sampleIndex * channels;

                        for (var channel = 0; channel < channels; channel++)
                        {
                            chunkData[dst + channel] = buffer[bufferPosition + channel];
                        }

                        bufferPosition += channels;
                        sampleIndex++;
                    }
                }

                if (buffered == 0)
                {
                    throw new InvalidOperationException(
                        $"Unexpected end of audio stream. Expected {chunkSize} frames, got {sampleIndex}.");
                }

                yield return new AudioChunk(chunkData.AsMemory(), i, chunkSize);
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(buffer);
        }
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(string fileName, int inputFrameCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(fileName);
        await foreach (var chunk in ReadAsync(fileStream, inputFrameCount, cancellationToken))
        {
            yield return chunk;
        }
    }
}
