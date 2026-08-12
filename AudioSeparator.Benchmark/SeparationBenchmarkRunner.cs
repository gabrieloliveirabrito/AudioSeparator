using System.Diagnostics;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Benchmark;

namespace AudioSeparator.Benchmark;

public static class SeparationBenchmarkRunner
{
    public static async Task<SeparationBenchmarkReport> RunAsync(
        IAudioSeparator separator,
        string inputPath,
        SeparationBenchmarkOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(separator);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentNullException.ThrowIfNull(options);

        for (var warmup = 0; warmup < options.WarmupRuns; warmup++)
        {
            var warmupSession = await separator.CreateSession(inputPath, cancellationToken);
            await warmupSession.RunAsync(cancellationToken);
        }

        var phases = new List<PhaseMeasurement>();
        ISeparationSession session;

        if (options.IncludeProbe)
        {
            var probeBefore = MemorySampler.Capture(options.ForceGcBetweenPhases);
            var probeStopwatch = Stopwatch.StartNew();
            session = await separator.CreateSession(inputPath, cancellationToken);
            probeStopwatch.Stop();
            var probeAfter = MemorySampler.Capture(options.ForceGcBetweenPhases);

            phases.Add(new PhaseMeasurement
            {
                Phase = SeparationBenchmarkPhase.Probe,
                Duration = probeStopwatch.Elapsed,
                MemoryBytesBefore = probeBefore.ManagedBytes,
                MemoryBytesAfter = probeAfter.ManagedBytes,
                WorkingSetBytesBefore = probeBefore.WorkingSetBytes,
                WorkingSetBytesAfter = probeAfter.WorkingSetBytes
            });
        }
        else
        {
            session = await separator.CreateSession(inputPath, cancellationToken);
        }

        var observer = new CollectingBenchmarkObserver(phases, options.ForceGcBetweenPhases);
        var result = await session.RunAsync(observer, cancellationToken);

        if (!string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            Directory.CreateDirectory(options.OutputDirectory);

            foreach (var (name, stem) in result.Stems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var extension = Path.GetExtension(name);
                var fileName = string.IsNullOrEmpty(extension)
                    ? Path.Combine(options.OutputDirectory, $"{name}.{result.Writer.PreferredExtension}")
                    : Path.Combine(options.OutputDirectory, name);

                var writeBefore = MemorySampler.Capture(options.ForceGcBetweenPhases);
                observer.NotifyPhaseStarted(SeparationBenchmarkPhase.StemWrite, name);

                var writeStopwatch = Stopwatch.StartNew();
                await stem.WriteToFileAsync(fileName, result.Writer, cancellationToken);
                writeStopwatch.Stop();

                var writeAfter = MemorySampler.Capture(options.ForceGcBetweenPhases);
                observer.NotifyPhaseCompleted(new PhaseMeasurement
                {
                    Phase = SeparationBenchmarkPhase.StemWrite,
                    StemName = name,
                    Duration = writeStopwatch.Elapsed,
                    MemoryBytesBefore = writeBefore.ManagedBytes,
                    MemoryBytesAfter = writeAfter.ManagedBytes,
                    WorkingSetBytesBefore = writeBefore.WorkingSetBytes,
                    WorkingSetBytesAfter = writeAfter.WorkingSetBytes
                });
            }
        }

        return BenchmarkReportBuilder.Build(phases, session.Source);
    }

    private sealed class CollectingBenchmarkObserver : ISeparationBenchmarkObserver
    {
        private readonly List<PhaseMeasurement> _phases;
        private readonly bool _forceGcBetweenPhases;

        public CollectingBenchmarkObserver(List<PhaseMeasurement> phases, bool forceGcBetweenPhases)
        {
            _phases = phases;
            _forceGcBetweenPhases = forceGcBetweenPhases;
        }

        public void OnPhaseStarted(SeparationBenchmarkPhase phase, string? stemName = null)
        {
            if (_forceGcBetweenPhases)
            {
                MemorySampler.Capture(forceGc: true);
            }
        }

        public void OnPhaseCompleted(PhaseMeasurement measurement)
        {
            _phases.Add(measurement);

            if (_forceGcBetweenPhases)
            {
                MemorySampler.Capture(forceGc: true);
            }
        }

        public void NotifyPhaseStarted(SeparationBenchmarkPhase phase, string? stemName = null) =>
            OnPhaseStarted(phase, stemName);

        public void NotifyPhaseCompleted(PhaseMeasurement measurement) =>
            OnPhaseCompleted(measurement);
    }
}
