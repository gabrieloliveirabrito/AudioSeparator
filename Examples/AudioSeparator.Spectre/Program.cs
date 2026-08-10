using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;
using Spectre.Console;

var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100.wav");

var builder = DemucsBuilder.Create()
.UseNAudio()
.ConfigureSessionOptions(options =>
{
    options.AppendExecutionProvider_CUDA(0x28A1);
    options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
});

using var separator = builder.Build(modelPath);

await AnsiConsole.Progress().StartAsync(async ctx =>
{
    await foreach (var task in separator.Separate(inputPath))
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
});