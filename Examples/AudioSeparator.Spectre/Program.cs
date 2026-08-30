using AudioSeparator.Abstractions;
using AudioSeparator.FFMPEG;
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;
using Spectre.Console;

var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100.wav");
var outputDirectory = Path.Combine(Environment.CurrentDirectory, "Outputs");

var builder = DemucsBuilder.Create(modelPath)
    .UseStemNames("drums", "bass", "other", "vocals")
    .WithOutputStem("vocals")
    .UseFFMPEG(options =>
    {
        options.OutputFormat = "mp3";
        options.OutputCodec = "libmp3lame";
    })
    .WithOverlapAdd()
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

    var filePath = Path.Combine(outputDirectory, "vocals.mp3");
    if (File.Exists(filePath))
    {
        File.Delete(filePath);
    }

    using var result = await session.RunAsync();
    using var file = File.Create(filePath);

    await using var encoded = await result.OpenStemEncodedStreamAsync("vocals");
    await encoded.CopyToAsync(file);

    var extension = result.Writer?.PreferredExtension ?? "wav";
    AnsiConsole.MarkupLine($"[green]Done[/]");
});
