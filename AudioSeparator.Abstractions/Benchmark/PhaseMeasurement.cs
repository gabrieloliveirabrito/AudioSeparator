namespace AudioSeparator.Abstractions.Benchmark;

public sealed class PhaseMeasurement
{
    public required SeparationBenchmarkPhase Phase { get; init; }
    public string? StemName { get; init; }
    public required TimeSpan Duration { get; init; }
    public long MemoryBytesBefore { get; init; }
    public long MemoryBytesAfter { get; init; }
    public long WorkingSetBytesBefore { get; init; }
    public long WorkingSetBytesAfter { get; init; }

    public long MemoryDelta => MemoryBytesAfter - MemoryBytesBefore;
    public long WorkingSetDelta => WorkingSetBytesAfter - WorkingSetBytesBefore;
}
