using AudioSeparator.Abstractions.Benchmark;
using AudioSeparator.Benchmark;
using AudioSeparator.FFMPEG;
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;

var options = BenchmarkCliOptions.Parse(args);

if (options.ShowHelp)
{
    BenchmarkCliOptions.PrintHelp();
    return;
}

if (!File.Exists(options.ModelPath))
{
    Console.Error.WriteLine($"Model not found: {options.ModelPath}");
    Environment.ExitCode = 1;
    return;
}

if (!File.Exists(options.InputPath))
{
    Console.Error.WriteLine($"Input not found: {options.InputPath}");
    Environment.ExitCode = 1;
    return;
}

var builder = DemucsBuilder.Create(options.ModelPath)
    .UseStemNames("drums", "bass", "other", "vocals")
    .ConfigureSessionOptions(sessionOptions =>
    {
        sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

        if (options.UseCuda)
        {
            sessionOptions.AppendExecutionProvider_CUDA(0);
        }
    });

if (options.UseFfmpeg)
{
    builder.UseFFMPEG(ffmpegOptions =>
    {
        ffmpegOptions.OutputFormat = "wav";
    });
}
else
{
    builder.UseNAudio();
}

using var separator = builder.Build();

var benchmarkOptions = new SeparationBenchmarkOptions
{
    OutputDirectory = options.OutputDirectory,
    ForceGcBetweenPhases = options.ForceGcBetweenPhases,
    WarmupRuns = options.WarmupRuns,
    IncludeProbe = options.IncludeProbe
};

var report = await SeparationBenchmarkRunner.RunAsync(
    separator,
    options.InputPath,
    benchmarkOptions);

switch (options.Format)
{
    case BenchmarkOutputFormat.Json:
        JsonBenchmarkReporter.Write(report);
        break;
    default:
        ConsoleBenchmarkReporter.Write(report);
        break;
}

internal enum BenchmarkOutputFormat
{
    Console,
    Json
}

internal sealed class BenchmarkCliOptions
{
    public required string ModelPath { get; init; }
    public required string InputPath { get; init; }
    public string? OutputDirectory { get; init; }
    public BenchmarkOutputFormat Format { get; init; } = BenchmarkOutputFormat.Console;
    public int WarmupRuns { get; init; }
    public bool ForceGcBetweenPhases { get; init; }
    public bool IncludeProbe { get; init; } = true;
    public bool UseFfmpeg { get; init; }
    public bool UseCuda { get; init; }
    public bool ShowHelp { get; init; }

    public static BenchmarkCliOptions Parse(string[] args)
    {
        var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
        var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100.wav");
        string? outputDirectory = null;
        var format = BenchmarkOutputFormat.Console;
        var warmupRuns = 0;
        var forceGcBetweenPhases = false;
        var includeProbe = true;
        var useFfmpeg = false;
        var useCuda = false;
        var showHelp = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help" or "-h":
                    showHelp = true;
                    break;
                case "--model":
                    modelPath = RequireValue(args, ref i, "--model");
                    break;
                case "--input":
                    inputPath = RequireValue(args, ref i, "--input");
                    break;
                case "--output":
                    outputDirectory = RequireValue(args, ref i, "--output");
                    break;
                case "--format":
                    format = ParseFormat(RequireValue(args, ref i, "--format"));
                    break;
                case "--warmup":
                    warmupRuns = int.Parse(RequireValue(args, ref i, "--warmup"));
                    break;
                case "--force-gc":
                    forceGcBetweenPhases = true;
                    break;
                case "--no-probe":
                    includeProbe = false;
                    break;
                case "--ffmpeg":
                    useFfmpeg = true;
                    break;
                case "--cuda":
                    useCuda = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        return new BenchmarkCliOptions
        {
            ModelPath = Path.GetFullPath(modelPath),
            InputPath = Path.GetFullPath(inputPath),
            OutputDirectory = outputDirectory is null ? null : Path.GetFullPath(outputDirectory),
            Format = format,
            WarmupRuns = warmupRuns,
            ForceGcBetweenPhases = forceGcBetweenPhases,
            IncludeProbe = includeProbe,
            UseFfmpeg = useFfmpeg,
            UseCuda = useCuda,
            ShowHelp = showHelp
        };
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
            AudioSeparator Benchmark

            Usage:
              dotnet run --project Examples/AudioSeparator.Benchmark -- [options]

            Options:
              --model <path>     ONNX model path (default: ../htdemucs.onnx)
              --input <path>     Input audio path (default: ../../44100.wav)
              --output <dir>     Write stems and benchmark stem write phase
              --format console   Output format: console or json (default: console)
              --warmup <n>       Warmup runs before measured run (default: 0)
              --force-gc         Force GC between phases for memory readings
              --no-probe         Skip probe/create-session timing
              --ffmpeg           Use FFMPEG I/O instead of NAudio
              --cuda             Enable CUDA execution provider
              -h, --help         Show this help
            """);
    }

    private static string RequireValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        index++;
        return args[index];
    }

    private static BenchmarkOutputFormat ParseFormat(string value) =>
        value.ToLowerInvariant() switch
        {
            "console" => BenchmarkOutputFormat.Console,
            "json" => BenchmarkOutputFormat.Json,
            _ => throw new ArgumentException($"Unsupported format: {value}. Use console or json.")
        };
}
