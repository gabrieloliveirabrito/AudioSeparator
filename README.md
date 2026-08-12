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
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseNAudio();

using var separator = builder.Build();
var session = await separator.CreateSession("input.wav");

foreach (var task in session.Tasks)
    task.SetProgressCallback((current, total) => { /* progress UI */ });

var result = await session.RunAsync();
var writer = NAudioExtensions.CreateWriter();
await result.WriteToDirectoryAsync("./Outputs", writer);
```

The separator **returns** separated stems (`SeparationResult`). Writing files is the consumer's responsibility via I/O extensions.

## Requirements

- .NET 10
- ONNX model file (not included in this repository)
- FFMPEG in `PATH` (optional, for FFMPEG reader/writer)
- CUDA (optional, for GPU ONNX execution)

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
