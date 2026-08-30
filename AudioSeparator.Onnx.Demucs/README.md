# AudioSeparator.Onnx.Demucs

> Demucs/htdemucs ONNX backend for AudioSeparator — separate audio into drums, bass, vocals, and other stems.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.Onnx.Demucs)](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.Onnx.Demucs
dotnet add package AudioSeparator.NAudio
```

Or swap NAudio for FFMPEG when you need broader format support:

```bash
dotnet add package AudioSeparator.FFMPEG
```

---

## When to use this package

- You want to **separate audio stems** using a Demucs or htdemucs ONNX model.
- This is the **recommended entry point** for most applications.
- Pair with [NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio) (simple WAV/MP3) or [FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) (any format ffmpeg supports).

---

## Core principle

> The separator **returns** `SeparationResult` with separated stems. It does **not** write files. Call `WriteToDirectoryAsync` after `RunAsync` to save stems to disk.

---

## Quick start

```csharp
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;

var builder = DemucsBuilder.Create("htdemucs.onnx")
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseNAudio()
    .ConfigureSessionOptions(options =>
    {
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        options.AppendExecutionProvider_CUDA(0); // optional GPU
    });

using var separator = builder.Build();
var session = await separator.CreateSession("input.wav");

foreach (var task in session.Tasks)
{
    task.SetProgressCallback((current, total) =>
        Console.WriteLine($"{task.Description}: {current}/{total}"));
}

var result = await session.RunAsync();
await result.WriteToDirectoryAsync("./Outputs");

foreach (var (name, _) in result.Stems)
{
    Console.WriteLine($"Wrote stem: {name}");
}
```

### FFMPEG output (e.g. MP3 stems)

```csharp
using AudioSeparator.FFMPEG;
using AudioSeparator.Onnx.Demucs;

var builder = DemucsBuilder.Create("htdemucs.onnx")
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseFFMPEG(options =>
    {
        options.OutputFormat = "mp3";
        options.OutputCodec = "libmp3lame";
    });
```

---

## Session flow

| Step | Action | Output |
|------|--------|--------|
| 1 | `CreateSession(path)` | Probe + validation |
| 2 | Attach progress on `session.Tasks` | UI feedback |
| 3 | `RunAsync()` | `SeparationResult` |
| 4 | `WriteToDirectoryAsync(dir)` | Files on disk |

---

## Key API

| Type | Role |
|------|------|
| `DemucsBuilder.Create(modelPath)` | Fluent entry point |
| `DemucsBuilder` | Builder: `UseStemNames`, `UseAudio`, `WithRequirements`, `ConfigureSessionOptions` |
| `DemucsSeparator` | `IAudioSeparator` implementation |
| `DemucsContext` | ONNX runtime context for Demucs |

Inherited from `OnnxSeparatorBuilder`: `.UseAudio()`, `.UseStemNames()`, `.WithRequirements()`, `.ConfigureSessionOptions()`.

---

## Requirements

- **.NET 10**
- **ONNX model file** (e.g. htdemucs) — not included in this package
- [AudioSeparator.Onnx](https://www.nuget.org/packages/AudioSeparator.Onnx) (transitive)
- **CUDA** (optional) — GPU inference via `ConfigureSessionOptions`
- **ffmpeg/ffprobe** (optional) — only if using [FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) extensions

---

## Examples

Sample projects in the [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator):

- `Examples/AudioSeparator.Console` — minimal console usage
- `Examples/AudioSeparator.Spectre` — progress bars with Spectre.Console
- `Examples/AudioSeparator.Benchmark` — performance benchmarking

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio) | NAudio I/O | WAV read/write |
| [AudioSeparator.FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) | FFMPEG I/O | Broad format support |
| [AudioSeparator.Benchmark](https://www.nuget.org/packages/AudioSeparator.Benchmark) | Benchmarking | Performance reports |
| [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions) | Contracts | Result types and extensions |

Source: [AudioSeparator.Onnx.Demucs on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.Onnx.Demucs)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
