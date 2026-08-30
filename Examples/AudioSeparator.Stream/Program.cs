using AudioSeparator.Abstractions;
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;

var modelPath = Path.Combine(Environment.CurrentDirectory, "..", "htdemucs.onnx");
var inputPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "44100.wav");
var outputPcmPath = Path.Combine(Environment.CurrentDirectory, "vocals.pcm");
var outputWavPath = Path.Combine(Environment.CurrentDirectory, "vocals.wav");
var enableOverlap = args.Contains("--overlap", StringComparer.OrdinalIgnoreCase);

using var separator = DemucsBuilder.Create(modelPath)
    .UseNAudio()
    .WithOutputStem("vocals")
    .WithOverlapAdd(enableOverlap)
    .ConfigureSessionOptions(options =>
    {
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
    })
    .Build();

var session = await separator.CreateSession(inputPath);
using var result = await session.RunAsync();

// Raw PCM (f32le) — for piping to another process
using var pcmFile = File.Create(outputPcmPath);
await result.CopyStemPcmToAsync("vocals", pcmFile);

// Encoded WAV via IAudioWriter — playable file
await using var encoded = await result.OpenStemEncodedStreamAsync("vocals");
await using var wavFile = File.Create(outputWavPath);
await encoded.CopyToAsync(wavFile);

Console.WriteLine($"Wrote {outputPcmPath} (raw PCM) and {outputWavPath} (encoded)");
