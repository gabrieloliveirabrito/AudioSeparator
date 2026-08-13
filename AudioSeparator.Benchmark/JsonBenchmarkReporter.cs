using System.Text.Json;
using System.Text.Json.Serialization;
using AudioSeparator.Abstractions.Benchmark;

namespace AudioSeparator.Benchmark;

public static class JsonBenchmarkReporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Serialize(SeparationBenchmarkReport report) =>
        JsonSerializer.Serialize(report, SerializerOptions);

    public static void Write(SeparationBenchmarkReport report, TextWriter? output = null)
    {
        output ??= Console.Out;
        output.WriteLine(Serialize(report));
    }
}
