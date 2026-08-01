using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Model;
using NAudio.Wave;

namespace AudioSeparator.NAudio;

public class NAudioWriter : IAudioWriter
{
    private void ResetStreamPosition(Stream input)
    {
        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }
    }

    public async Task Write(Stream destination, IAudioChunk[] chunks, IModelMetadata modelMetadata)
    {
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(modelMetadata.AudioFrequency, modelMetadata.OutputChannels);

        using (var memory = new MemoryStream())
        using (var rawStream = new RawSourceWaveStream(memory, waveFormat))
        {
            foreach (var chunk in chunks.OrderBy(c => c.Index))
            {
                var buffer = new float[chunk.Length];
                for (int sampleIndex = 0; sampleIndex < chunk.Samples.Length; sampleIndex++)
                {
                    float[] sampleData = chunk.Samples[sampleIndex];
                    for (int channelIndex = 0; channelIndex < modelMetadata.OutputChannels; channelIndex++)
                    {
                        buffer[sampleIndex + channelIndex] = sampleData[channelIndex];
                    }
                }

                var waveBuffer = new WaveBuffer(buffer.Length * 4);
                Array.Copy(buffer, waveBuffer.FloatBuffer, buffer.Length);

                await rawStream.WriteAsync(waveBuffer.ByteBuffer, 0, waveBuffer.ByteBufferCount);
            }

            memory.Position = 0;
            await memory.CopyToAsync(destination);
        }
    }

    public async Task Write(string fileName, IAudioChunk[] chunks, IModelMetadata modelMetadata)
    {
        using var fileStream = File.Create(fileName);

        await Write(fileStream, chunks, modelMetadata);
    }
}