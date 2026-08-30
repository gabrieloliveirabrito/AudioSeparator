using System.Diagnostics;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Benchmark;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Core.Audio;
using AudioSeparator.Core.Tasks;

namespace AudioSeparator.Core;

public sealed class SeparationSession : ISeparationSession
{
    private readonly AudioSeparatorContext _context;
    private readonly IReadOnlyList<IProcessTask> _tasks;

    public SeparationSession(
        AudioSeparatorContext context,
        IReadOnlyList<IProcessTask> tasks,
        AudioSourceInfo source)
    {
        _context = context;
        _tasks = tasks;
        Source = source;
    }

    public AudioSourceInfo Source { get; }

    public IReadOnlyList<IProcessTask> Tasks => _tasks;

    public async Task RunTasksAsync(CancellationToken cancellationToken = default)
    {
        foreach (var task in _tasks)
        {
            await task.ExecuteAsync(cancellationToken).WaitAsync(cancellationToken);
        }
    }

    public SeparationResult AssembleResult()
    {
        if (_context.SourceInfo is null)
        {
            throw new InvalidOperationException("Source metadata is missing.");
        }

        var channels = _context.InferenceSpec?.OutputChannels ?? _context.SourceInfo.Channels;
        var outputSampleRate = _context.Requirements.SampleRate > 0
            ? _context.Requirements.SampleRate
            : _context.SourceInfo.SampleRate;

        var stems = new Dictionary<string, StemAudio>();

        foreach (var (name, samples) in _context.OutputStemSamples)
        {
            stems[name] = new StemAudio
            {
                Name = name,
                SampleRate = _context.Requirements.SampleRate,
                Channels = channels,
                Audio = StemAudioBuffer.CreatePcmStream(samples)
            };
        }

        return new SeparationResult
        {
            Source = _context.SourceInfo,
            Stems = stems,
            Writer = _context.AudioWriter
        };
    }

    public Task<SeparationResult> RunAsync(CancellationToken cancellationToken) =>
        RunAsyncInternal(observer: null, cancellationToken);

    public Task<SeparationResult> RunAsync(
        ISeparationBenchmarkObserver observer,
        CancellationToken cancellationToken = default) =>
        RunAsyncInternal(observer, cancellationToken);

    private async Task<SeparationResult> RunAsyncInternal(
        ISeparationBenchmarkObserver? observer,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var task in _tasks)
            {
                if (observer is null)
                {
                    await task.ExecuteAsync(cancellationToken).WaitAsync(cancellationToken);
                    continue;
                }

                var phase = ResolvePhase(task);
                var before = CaptureMemorySnapshot();
                observer.OnPhaseStarted(phase);

                var stopwatch = Stopwatch.StartNew();
                await task.ExecuteAsync(cancellationToken).WaitAsync(cancellationToken);
                stopwatch.Stop();

                var after = CaptureMemorySnapshot();
                observer.OnPhaseCompleted(new PhaseMeasurement
                {
                    Phase = phase,
                    Duration = stopwatch.Elapsed,
                    MemoryBytesBefore = before.ManagedBytes,
                    MemoryBytesAfter = after.ManagedBytes,
                    WorkingSetBytesBefore = before.WorkingSetBytes,
                    WorkingSetBytesAfter = after.WorkingSetBytes
                });
            }

            if (observer is null)
            {
                return AssembleResult();
            }

            var assemblyBefore = CaptureMemorySnapshot();
            observer.OnPhaseStarted(SeparationBenchmarkPhase.ResultAssembly);

            var assemblyStopwatch = Stopwatch.StartNew();
            var result = AssembleResult();
            assemblyStopwatch.Stop();

            var assemblyAfter = CaptureMemorySnapshot();
            observer.OnPhaseCompleted(new PhaseMeasurement
            {
                Phase = SeparationBenchmarkPhase.ResultAssembly,
                Duration = assemblyStopwatch.Elapsed,
                MemoryBytesBefore = assemblyBefore.ManagedBytes,
                MemoryBytesAfter = assemblyAfter.ManagedBytes,
                WorkingSetBytesBefore = assemblyBefore.WorkingSetBytes,
                WorkingSetBytesAfter = assemblyAfter.WorkingSetBytes
            });

            return result;
        }
        finally
        {
            _context.DisposableResource?.Dispose();
        }
    }

    private static SeparationBenchmarkPhase ResolvePhase(IProcessTask task) =>
        task is AudioReadTask
            ? SeparationBenchmarkPhase.AudioRead
            : SeparationBenchmarkPhase.Inference;

    private static (long ManagedBytes, long WorkingSetBytes) CaptureMemorySnapshot()
    {
        var managedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var workingSetBytes = Process.GetCurrentProcess().WorkingSet64;
        return (managedBytes, workingSetBytes);
    }
}
