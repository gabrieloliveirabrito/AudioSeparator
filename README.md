# AudioSeparator

Modular .NET library for audio stem separation. Supports pluggable audio readers (NAudio, FFMPEG), ONNX-based backends (including Demucs/htdemucs), and optional write extensions.

## Packages

| Package | Role |
|---------|------|
| `AudioSeparator.Abstractions` | Contracts and DTOs |
| `AudioSeparator.Core` | Session API and task pipeline |
| `AudioSeparator.Onnx` | ONNX session and inference |
| `AudioSeparator.Onnx.Demucs` | Demucs ONNX backend |
| `AudioSeparator.NAudio` | NAudio reader + write extensions |
| `AudioSeparator.FFMPEG` | FFMPEG reader + write extensions |

## Quick start

```csharp
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;

var builder = DemucsBuilder.Create("model.onnx")
    .UseNAudio();

using var separator = builder.Build();
var session = await separator.CreateSession("input.wav");

foreach (var task in session.Tasks)
    task.SetProgressCallback((current, total) => { /* progress UI */ });

var result = await session.RunAsync();
await result.WriteToDirectoryAsync("./Outputs");
```

The separator **returns** separated stems (`SeparationResult`). Writing files uses the writer registered at build time via `UseNAudio()` or `UseFFMPEG()`:

```csharp
await result.WriteToDirectoryAsync("./Outputs");
```

## Requirements

- .NET 10
- ONNX model file (not included in this repository)
- FFMPEG in `PATH` (optional, for FFMPEG reader/writer)
- CUDA (optional, for GPU ONNX execution)

## Audio format

ONNX models expose tensor dimensions (channels, frame count, stem count) but not sample rate. Use a model-specific builder when available:

| Builder | Sample rate | Default stem names |
|---------|-------------|-------------------|
| `DemucsBuilder` | 44100 Hz | drums, bass, other, vocals |

For other ONNX models without a dedicated package, set requirements explicitly via `WithRequirements`:

```csharp
.WithRequirements(new SeparationRequirements
{
    SampleRate = 48000,
    StemNames = ["vocals", "instrumental"]
})
```

When `SampleRate` is zero (the default), sample rate is not validated and output stems use the source file rate.

## Build

```bash
sh restore-all.sh
sh build-all.sh
```

## Examples

- `Examples/AudioSeparator.Console` — minimal console usage
- `Examples/AudioSeparator.Spectre` — progress bars with Spectre.Console

## Architecture

See [`.cursor/skills/audio-separator/SKILL.md`](.cursor/skills/audio-separator/SKILL.md) for layer map and conventions.
