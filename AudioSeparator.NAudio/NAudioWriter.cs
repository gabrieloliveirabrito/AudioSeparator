using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Model;
using NAudio.Utils;
using NAudio.Wave;

namespace AudioSeparator.NAudio;

public class NAudioWriter : IAudioWriter
{
    public async Task WriteAsync(Stream destination, AudioChunk[] chunks, ModelMetadata modelMetadata, CancellationToken cancellationToken = default)
    {
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(modelMetadata.AudioFrequency, modelMetadata.OutputChannels);

        using var ignoreDispose = new IgnoreDisposeStream(destination);
        using var wavStream = new WaveFileWriter(ignoreDispose, waveFormat);

        foreach (var chunk in chunks.OrderBy(c => c.Index))
        {
            for (int sampleIndex = 0; sampleIndex < chunk.Samples.Length; sampleIndex += modelMetadata.OutputChannels)
            {
                for (int channelIndex = 0; channelIndex < modelMetadata.OutputChannels; channelIndex++)
                {
                    var sample = chunk.Samples.Span[sampleIndex + channelIndex];
                    wavStream.WriteSample(sample);
                }
            }
        }
    }

    public async Task WriteAsync(string fileName, AudioChunk[] chunks, ModelMetadata modelMetadata, CancellationToken cancellationToken = default)
    {
        using var memory = new MemoryStream();
        await WriteAsync(memory, chunks, modelMetadata);

        memory.Seek(0, SeekOrigin.Begin);
        using var fileStream = File.Create(fileName);

        await memory.CopyToAsync(fileStream);
    }
}