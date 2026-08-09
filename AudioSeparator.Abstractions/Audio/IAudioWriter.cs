using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Abstractions.Audio;

public interface IAudioWriter
{
    Task WriteAsync(Stream destination, AudioChunk[] chunks, ModelMetadata modelMetadata, CancellationToken cancellationToken = default);
    Task WriteAsync(string fileName, AudioChunk[] chunks, ModelMetadata modelMetadata, CancellationToken cancellationToken = default);
}