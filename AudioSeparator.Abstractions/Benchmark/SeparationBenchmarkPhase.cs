namespace AudioSeparator.Abstractions.Benchmark;

public enum SeparationBenchmarkPhase
{
    Probe,
    AudioRead,
    Inference,
    ResultAssembly,
    StemWrite
}
