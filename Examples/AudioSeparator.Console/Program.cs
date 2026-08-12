namespace AudioSeparator.Console;

using AudioSeparator.FFMPEG;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;

public class Program
{
    public static async Task Main(string[] args)
    {
        var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
        var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100-full.wav");
        var outputDirectory = Path.Combine(Environment.CurrentDirectory, "Outputs");

        var builder = DemucsBuilder.Create(modelPath)
            .UseStemNames("drums", "bass", "other", "vocals")
            .UseFFMPEG(options =>
            {
                options.OutputFormat = "mp3";
            })
            .ConfigureSessionOptions(options =>
            {
                options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            });

        using var separator = builder.Build();
        var session = await separator.CreateSession(inputPath);

        foreach (var task in session.Tasks)
        {
            global::System.Console.WriteLine(task.Description);
        }

        var result = await session.RunAsync();
        var writer = FFMPEGExtensions.CreateWriter(options => options.OutputFormat = "mp3");
        await result.WriteToDirectoryAsync(outputDirectory, writer);

        foreach (var (name, stem) in result.Stems)
        {
            global::System.Console.WriteLine($"Wrote stem: {name}");
        }
    }
}
