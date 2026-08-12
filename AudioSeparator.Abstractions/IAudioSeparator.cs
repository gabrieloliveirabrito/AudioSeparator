namespace AudioSeparator.Abstractions;

public interface IAudioSeparator : IDisposable
{
    Task<ISeparationSession> CreateSession(string inputPath, CancellationToken cancellationToken = default);
}
