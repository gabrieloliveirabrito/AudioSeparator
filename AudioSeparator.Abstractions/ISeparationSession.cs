using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Benchmark;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Abstractions;

public interface ISeparationSession
{
    AudioSourceInfo Source { get; }
    IReadOnlyList<IProcessTask> Tasks { get; }
    Task RunTasksAsync(CancellationToken cancellationToken = default);
    SeparationResult AssembleResult();
    Task<SeparationResult> RunAsync(CancellationToken cancellationToken = default);
    Task<SeparationResult> RunAsync(
        ISeparationBenchmarkObserver observer,
        CancellationToken cancellationToken = default);
}
