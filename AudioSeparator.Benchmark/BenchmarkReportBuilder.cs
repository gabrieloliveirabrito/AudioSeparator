using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Benchmark;

namespace AudioSeparator.Benchmark;

public static class BenchmarkReportBuilder
{
    public static SeparationBenchmarkReport Build(
        IReadOnlyList<PhaseMeasurement> phases,
        AudioSourceInfo source)
    {
        var totalDuration = phases.Aggregate(TimeSpan.Zero, (sum, phase) => sum + phase.Duration);
        var audioDurationSeconds = source.SampleRate > 0
            ? (double)source.SampleCount / source.SampleRate
            : 0;

        var realTimeFactor = audioDurationSeconds > 0
            ? totalDuration.TotalSeconds / audioDurationSeconds
            : 0;

        var samplesPerSecond = totalDuration.TotalSeconds > 0
            ? source.SampleCount / totalDuration.TotalSeconds
            : 0;

        return new SeparationBenchmarkReport
        {
            Source = source,
            Phases = phases,
            TotalDuration = totalDuration,
            RealTimeFactor = realTimeFactor,
            SamplesPerSecond = samplesPerSecond,
            AudioDurationSeconds = audioDurationSeconds
        };
    }
}
