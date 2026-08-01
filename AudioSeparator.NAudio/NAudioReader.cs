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
                return new WaveFileReader(input);
            }

            throw new NotSupportedException("Unsupported audio stream format", ex);
        }
    }

    public Task<IAudioChunk[]> Read(Stream input, IModelMetadata modelMetadata)
    {
        using var reader = CreateWaveStream(input);
        if (reader.WaveFormat.SampleRate != modelMetadata.AudioFrequency)
        {
            throw new InvalidOperationException($"The model expect {modelMetadata.AudioFrequency} sample rate, but the file has {reader.WaveFormat.SampleRate}.");
        }

        if (reader.WaveFormat.Channels != modelMetadata.InputChannels)
        {
            throw new InvalidOperationException($"The model expect {modelMetadata.InputChannels} channels, but rethe file has {reader.WaveFormat.Channels}.");
        }

        var provider = reader.ToSampleProvider();

        var totalSamples = reader.Length / reader.WaveFormat.BlockAlign;
        var readChunks = (int)Math.Ceiling(totalSamples / (double)modelMetadata.InputSize);
        var chunks = new IAudioChunk[readChunks];
        var buffer = new float[reader.WaveFormat.Channels * reader.WaveFormat.SampleRate];

        for (int i = 0; i < readChunks; i++)
        {
            int chunkOffset = modelMetadata.InputSize * i;
            int chunkSize = (int)Math.Min(modelMetadata.InputSize, totalSamples - chunkOffset);
            float[][] chunkData = new float[chunkSize][];
            int readed = 0, sampleIndex = 0;

            reader.Position = chunkOffset * reader.WaveFormat.BlockAlign;
            while ((readed = provider.Read(buffer, 0, buffer.Length)) != 0)
            {
                for (var j = 0; j < readed && sampleIndex < chunkSize; j += modelMetadata.InputChannels)
                {
                    if (chunkData[j] is null)
                    {
                        chunkData[j] = new float[modelMetadata.InputChannels];
                    }

                    for (var k = 0; k < modelMetadata.InputChannels; k++)
                    {
                        chunkData[sampleIndex][k] = buffer[j + k];
                    }

                    sampleIndex++;
                }

                if (sampleIndex == chunkSize)
                {
                    break;
                }
            }

            chunks[i] = new NAudioChunk(chunkData, i, chunkSize);
        }

        return Task.FromResult(chunks);
    }

    public async Task<IAudioChunk[]> Read(string fileName, IModelMetadata modelMetadata)
    {
        using var fileStream = File.OpenRead(fileName);
        return await Read(fileStream, modelMetadata);
    }
}