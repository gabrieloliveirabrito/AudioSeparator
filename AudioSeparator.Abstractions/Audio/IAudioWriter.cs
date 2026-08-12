namespace AudioSeparator.Abstractions.Audio;

public interface IAudioWriter
{
    Task WriteAsync(Stream destination, global::AudioSeparator.Abstractions.StemAudio stem, CancellationToken cancellationToken = default);
    Task WriteAsync(string fileName, global::AudioSeparator.Abstractions.StemAudio stem, CancellationToken cancellationToken = default);
}
