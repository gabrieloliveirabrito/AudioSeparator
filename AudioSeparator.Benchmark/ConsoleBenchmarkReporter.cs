using AudioSeparator.Abstractions.Benchmark;

namespace AudioSeparator.Benchmark;

public static class ConsoleBenchmarkReporter
{
    public static void Write(SeparationBenchmarkReport report, TextWriter? output = null)
    {
        output ??= Console.Out;

        output.WriteLine("Phase              Duration    Memory Δ    % Total");
        output.WriteLine(new string('─', 49));

        foreach (var phase in report.Phases)
        {
            var label = FormatPhaseLabel(phase);
            var duration = FormatDuration(phase.Duration);
            var memoryDelta = FormatBytes(phase.MemoryDelta);
            var percent = report.TotalDuration.TotalMilliseconds > 0
                ? phase.Duration.TotalMilliseconds / report.TotalDuration.TotalMilliseconds * 100
                : 0;

            output.WriteLine($"{label,-18} {duration,10}  {memoryDelta,9}  {percent,5:F0}%");
        }

        output.WriteLine(new string('─', 49));
        output.WriteLine(
            $"Total              {FormatDuration(report.TotalDuration),10}  RTF: {report.RealTimeFactor:F2}x  " +
            $"({report.SamplesPerSecond:N0} samples/s)");
        output.WriteLine(
            $"Audio duration: {report.AudioDurationSeconds:F2}s  Chunks: {report.Source.ChunkCount}  " +
            $"Sample rate: {report.Source.SampleRate} Hz");
    }

    private static string FormatPhaseLabel(PhaseMeasurement phase) =>
        phase.Phase switch
        {
            SeparationBenchmarkPhase.StemWrite when !string.IsNullOrEmpty(phase.StemName) =>
                $"StemWrite:{phase.StemName}",
            _ => phase.Phase.ToString()
        };

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalMilliseconds >= 1000
            ? $"{duration.TotalSeconds:F2}s"
            : $"{duration.TotalMilliseconds:F0} ms";

    private static string FormatBytes(long bytes)
    {
        var sign = bytes >= 0 ? "+" : "-";
        var absolute = Math.Abs(bytes);

        if (absolute >= 1024 * 1024)
        {
            return $"{sign}{absolute / (1024.0 * 1024.0):F1} MB";
        }

        if (absolute >= 1024)
        {
            return $"{sign}{absolute / 1024.0:F1} KB";
        }

        return $"{sign}{absolute} B";
    }
}
