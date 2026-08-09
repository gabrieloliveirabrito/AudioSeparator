using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Abstractions.Audio;

public interface IAudioReader
{
    IAsyncEnumerable<AudioChunk> ReadAsync(Stream input, int inputSize, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AudioChunk> ReadAsync(string fileName, int inputSize, CancellationToken cancellationToken = default);
}