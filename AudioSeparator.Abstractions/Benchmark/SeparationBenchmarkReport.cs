using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions.Benchmark;

public sealed class SeparationBenchmarkReport
{
    public required AudioSourceInfo Source { get; init; }
    public required IReadOnlyList<PhaseMeasurement> Phases { get; init; }
    public required TimeSpan TotalDuration { get; init; }
    public double RealTimeFactor { get; init; }
    public double SamplesPerSecond { get; init; }
    public double AudioDurationSeconds { get; init; }
}
