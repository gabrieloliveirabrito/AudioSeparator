namespace AudioSeparator.Abstractions.Audio;

public interface IAudioWriter
{
    string PreferredExtension { get; }

    Task WriteAsync(Stream destination, global::AudioSeparator.Abstractions.StemAudio stem, CancellationToken cancellationToken = default);
    Task WriteAsync(string fileName, global::AudioSeparator.Abstractions.StemAudio stem, CancellationToken cancellationToken = default);
}
