namespace AudioSeparator.Console;

using AudioSeparator.Core;
using AudioSeparator.FFMPEG;
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;

public class Program
{
    public static async Task Main(string[] args)
    {
        var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
        var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100-full.wav");

        var builder = DemucsBuilder.Create()
        .UseNAudio()
        //.UseFFMPEG()
        .ConfigureSessionOptions(options =>
        {
            options.AppendExecutionProvider_CUDA(0x28A1);
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        });

        using var separator = builder.Build(modelPath);
        await foreach (var task in separator.Separate(inputPath))
        {
            System.Console.WriteLine(task.Description);
        }
    }
}