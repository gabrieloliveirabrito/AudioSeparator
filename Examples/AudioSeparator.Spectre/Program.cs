using AudioSeparator.Abstractions;
using AudioSeparator.NAudio;
using AudioSeparator.FFMPEG;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;
using Spectre.Console;

var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100.wav");
var outputDirectory = Path.Combine(Environment.CurrentDirectory, "Outputs");

var builder = DemucsBuilder.Create(modelPath)
    .UseStemNames("drums", "bass", "other", "vocals")
    //.UseNAudio()
    .UseFFMPEG(options => {
        options.OutputFormat = "mp3";
        options.OutputCodec = "libmp3lame";
    })
    .ConfigureSessionOptions(options =>
    {
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        options.AppendExecutionProvider_CUDA(0);
    });

using var separator = builder.Build();
var session = await separator.CreateSession(inputPath);

await AnsiConsole.Progress().StartAsync(async ctx =>
{
    foreach (var task in session.Tasks)
    {
        var progressTask = ctx.AddTask(task.Description);
        task.SetProgressCallback((current, total) =>
        {
            progressTask.Value = current;
            progressTask.MaxValue = total;

            if (current == total)
            {
                progressTask.StopTask();
            }
        });
    }

    var result = await session.RunAsync();
    await result.WriteToDirectoryAsync(outputDirectory);

    AnsiConsole.MarkupLine($"[green]Wrote {result.Stems.Count} stems to {outputDirectory}[/]");
});
