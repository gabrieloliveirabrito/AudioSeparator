namespace AudioSeparator.Abstractions.Benchmark;

public sealed class SeparationBenchmarkOptions
{
    public string? OutputDirectory { get; init; }
    public bool ForceGcBetweenPhases { get; init; }
    public int WarmupRuns { get; init; }
    public bool IncludeProbe { get; init; } = true;
}
