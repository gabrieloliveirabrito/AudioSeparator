namespace AudioSeparator.Abstractions.Audio;

public interface IAudioReader
{
    Task<AudioSourceInfo> ProbeAsync(Stream input, int inputFrameCount, CancellationToken cancellationToken = default);
    Task<AudioSourceInfo> ProbeAsync(string fileName, int inputFrameCount, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AudioChunk> ReadAsync(Stream input, int inputFrameCount, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AudioChunk> ReadAsync(string fileName, int inputFrameCount, CancellationToken cancellationToken = default);
}
