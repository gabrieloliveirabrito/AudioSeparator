using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;
using Spectre.Console;

var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "mirai4.wav");
var outputDirectory = Path.Combine(Environment.CurrentDirectory, "Outputs");

var builder = DemucsBuilder.Create(modelPath)
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseNAudio()
    .ConfigureSessionOptions(options =>
    {
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
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
    var writer = NAudioExtensions.CreateWriter();
    await result.WriteToDirectoryAsync(outputDirectory, writer);

    AnsiConsole.MarkupLine($"[green]Wrote {result.Stems.Count} stems to {outputDirectory}[/]");
});
