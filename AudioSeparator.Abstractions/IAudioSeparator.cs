using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions;

public interface IAudioSeparator : IDisposable
{
    Task<ISeparationSession> CreateSession(string inputPath, CancellationToken cancellationToken = default);

    Task<ISeparationSession> CreateSession(Stream input, CancellationToken cancellationToken = default);

    Task<ISeparationSession> CreateSession(
        Stream input,
        AudioSourceInfo sourceInfo,
        CancellationToken cancellationToken = default);
}
