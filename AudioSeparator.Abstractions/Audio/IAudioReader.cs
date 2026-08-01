using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Abstractions.Audio;

public interface IAudioReader
{
    Task<IAudioChunk[]> Read(Stream input, IModelMetadata modelMetadata);
    Task<IAudioChunk[]> Read(string fileName, IModelMetadata modelMetadata);
}