using System.Runtime.InteropServices;
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

        var pcmBytes = StemAudioStream.ReadPcmBytes(stem.Audio);
        var samples = MemoryMarshal.Cast<byte, float>(pcmBytes.AsSpan());
        foreach (var sample in samples)
        {
            wavStream.WriteSample(sample);
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
