using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using NAudio.Utils;
using NAudio.Wave;

namespace AudioSeparator.NAudio;

public class NAudioWriter : IAudioWriter
{
    public string PreferredExtension => "wav";
    public Task WriteAsync(Stream destination, StemAudio stem, CancellationToken cancellationToken = default)
    {
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(stem.SampleRate, stem.Channels);

        using var ignoreDispose = new IgnoreDisposeStream(destination);
        using var wavStream = new WaveFileWriter(ignoreDispose, waveFormat);

        foreach (var chunk in stem.Chunks.OrderBy(c => c.Index))
        {
            for (var sampleIndex = 0; sampleIndex < chunk.Samples.Length; sampleIndex += stem.Channels)
            {
                for (var channelIndex = 0; channelIndex < stem.Channels; channelIndex++)
                {
                    wavStream.WriteSample(chunk.Samples.Span[sampleIndex + channelIndex]);
                }
            }
        }

        return Task.CompletedTask;
    }

    public async Task WriteAsync(string fileName, StemAudio stem, CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        await WriteAsync(memory, stem, cancellationToken);

        memory.Seek(0, SeekOrigin.Begin);
        await using var fileStream = File.Create(fileName);
        await memory.CopyToAsync(fileStream, cancellationToken);
    }
}
