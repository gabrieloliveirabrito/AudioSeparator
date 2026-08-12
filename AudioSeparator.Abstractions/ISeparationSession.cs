using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Abstractions;

public interface ISeparationSession
{
    AudioSourceInfo Source { get; }
    IReadOnlyList<IProcessTask> Tasks { get; }
    Task<SeparationResult> RunAsync(CancellationToken cancellationToken = default);
}
