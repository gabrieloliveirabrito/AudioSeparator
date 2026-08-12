namespace AudioSeparator.Abstractions.Benchmark;

public interface ISeparationBenchmarkObserver
{
    void OnPhaseStarted(SeparationBenchmarkPhase phase, string? stemName = null);
    void OnPhaseCompleted(PhaseMeasurement measurement);
}
