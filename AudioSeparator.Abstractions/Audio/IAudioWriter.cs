using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Abstractions.Audio;

public interface IAudioWriter
{
    Task Write(Stream destination, IAudioChunk[] chunks, IModelMetadata modelMetadata);
    Task Write(string fileName, IAudioChunk[] chunks, IModelMetadata modelMetadata);
}