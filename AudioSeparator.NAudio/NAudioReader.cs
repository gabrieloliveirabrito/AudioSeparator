using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Model;
using NAudio.Wave;

namespace AudioSeparator.NAudio;

public class NAudioReader : IAudioReader
{
    private void ResetStreamPosition(Stream input)
    {
        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }
    }

    private WaveStream CreateWaveStream(Stream input)
    {
        try
        {
            ResetStreamPosition(input);
            return new StreamMediaFoundationReader(input);
        }
        catch (Exception ex)
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

            throw new NotSupportedException("Unsupported audio stream format", ex);
        }
    }

    private Task<AudioMetadata> ReadMetadataAsync(Stream input, int inputSize)
    {
        using var reader = CreateWaveStream(input);
        var waveFormat = reader.WaveFormat;

        var frames = reader.Length / waveFormat.BlockAlign;
        var chunkCount = (int)Math.Ceiling(frames / (double)inputSize);

        var metadata = new AudioMetadata
        {
            SampleRate = waveFormat.SampleRate,
            SampleCount = frames,
            Channels = waveFormat.Channels,
            ChunkCount = chunkCount
        };

        return Task.FromResult(metadata);
    }

    public async IAsyncEnumerable<AudioChunk> ReadAsync(Stream input, int inputSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = ArrayPool<float>.Shared.Rent(8192);
        try
        {
            using var reader = CreateWaveStream(input);
            var channels = reader.WaveFormat.Channels;

            var frames = reader.Length / reader.WaveFormat.BlockAlign;
            var chunkCount = (int)Math.Ceiling(frames / (double)inputSize);
            // if (reader.WaveFormat.SampleRate != modelMetadata.AudioFrequency)
            // {
            //     throw new InvalidOperationException($"The model expect {modelMetadata.AudioFrequency} sample rate, but the file has {audioMetadata.SampleRate}.");
            // }

            // if (reader.WaveFormat.Channels != modelMetadata.InputChannels)
            // {
            //     throw new InvalidOperationException($"The model expect {modelMetadata.InputChannels} channels, but rethe file has {audioMetadata.Channels}.");
            // }

            var provider = reader.ToSampleProvider();

            int buffered = 0;
            int bufferPosition = 0;

            for (int i = 0; i < chunkCount; i++)
            {
                var chunkOffset = inputSize * i;
                var chunkSize = Math.Min(inputSize, (int)frames - chunkOffset);

                var chunkData = new float[chunkSize * channels];
                int sampleIndex = 0;

                while (sampleIndex < chunkSize)
                {
                    if (bufferPosition >= buffered)
                    {
                        buffered = provider.Read(buffer, 0, buffer.Length);
                        bufferPosition = 0;

                        if (buffered == 0)
                            break;
                    }

                    while(bufferPosition < buffered && sampleIndex < chunkSize)
                    {
                        var dst = sampleIndex * channels;

                        for (int channel = 0; channel < channels; channel++)
                        {
                            chunkData[dst + channel] = buffer[bufferPosition + channel];
                        }

                        bufferPosition += channels;
                        sampleIndex++;
                    }
                }

                if (buffered == 0)
                {
                    throw new InvalidOperationException($"Unexpected end of audio stream. Expected {chunkSize} frames, got {sampleIndex}.");
                }

                yield return new AudioChunk(chunkData.AsMemory(), i, chunkSize);
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(buffer);
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