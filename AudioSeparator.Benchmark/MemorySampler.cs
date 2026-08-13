namespace AudioSeparator.Benchmark;

public static class MemorySampler
{
    public static MemorySnapshot Capture(bool forceGc = false)
    {
        if (forceGc)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return new MemorySnapshot(
            GC.GetTotalMemory(forceFullCollection: false),
            System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
    }
}

public readonly record struct MemorySnapshot(long ManagedBytes, long WorkingSetBytes);
