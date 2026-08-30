# AudioSeparator

> Modular .NET library for audio stem separation — pluggable readers (NAudio, FFMPEG), ONNX backends (Demucs/htdemucs), and optional write extensions.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

Separate audio into stems (drums, bass, vocals, other) using ONNX models. The library is split into small NuGet packages so you reference only what you need.

---

## Packages

| Package | NuGet | README |
|---------|-------|--------|
| **AudioSeparator.Onnx.Demucs** | [nuget.org](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | [README](AudioSeparator.Onnx.Demucs/README.md) |
| AudioSeparator.Abstractions | [nuget.org](https://www.nuget.org/packages/AudioSeparator.Abstractions) | [README](AudioSeparator.Abstractions/README.md) |
| AudioSeparator.Core | [nuget.org](https://www.nuget.org/packages/AudioSeparator.Core) | [README](AudioSeparator.Core/README.md) |
| AudioSeparator.Onnx | [nuget.org](https://www.nuget.org/packages/AudioSeparator.Onnx) | [README](AudioSeparator.Onnx/README.md) |
| AudioSeparator.NAudio | [nuget.org](https://www.nuget.org/packages/AudioSeparator.NAudio) | [README](AudioSeparator.NAudio/README.md) |
| AudioSeparator.FFMPEG | [nuget.org](https://www.nuget.org/packages/AudioSeparator.FFMPEG) | [README](AudioSeparator.FFMPEG/README.md) |
| AudioSeparator.Benchmark | [nuget.org](https://www.nuget.org/packages/AudioSeparator.Benchmark) | [README](AudioSeparator.Benchmark/README.md) |

**Start here:** [AudioSeparator.Onnx.Demucs](AudioSeparator.Onnx.Demucs/README.md) for end-user separation.

---

## Architecture

```
Abstractions  ← contracts only, zero NuGet deps
Core          ← pipeline, session API, tasks
Onnx          ← InferenceSession, InferenceSpec, OnnxInferenceTask
Onnx.Demucs   ← Demucs ONNX backend
FFMPEG/NAudio ← IAudioReader + write extensions (optional persistence)
```

| Layer | Responsibility |
|-------|----------------|
| Abstractions | Interfaces, DTOs, result extensions |
| Core | `CreateSession` → probe → `RunAsync` → `SeparationResult` |
| Onnx | Generic ONNX inference pipeline |
| Onnx.Demucs | Demucs/htdemucs entry point |
| NAudio / FFMPEG | Read input, write stems to disk |

The separator **returns** separated stems. Writing files uses `WriteToDirectoryAsync` after `RunAsync`, via the writer registered at build time.

---

## Quick start

```bash
dotnet add package AudioSeparator.Onnx.Demucs
dotnet add package AudioSeparator.NAudio
```

```csharp
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;

var builder = DemucsBuilder.Create("htdemucs.onnx")
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseNAudio();

using var separator = builder.Build();
var session = await separator.CreateSession("input.wav");
var result = await session.RunAsync();
await result.WriteToDirectoryAsync("./Outputs");
```

See [AudioSeparator.Onnx.Demucs/README.md](AudioSeparator.Onnx.Demucs/README.md) for progress callbacks, CUDA, and FFMPEG output.

---

## Requirements

- **.NET 10**
- ONNX model file (not included in this repository)
- **FFMPEG** in `PATH` or `FFMPEG_PATH` (optional — only for FFMPEG reader/writer)
- **CUDA** (optional — for GPU ONNX execution)

---

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

---

## Examples

| Project | Description |
|---------|-------------|
| `Examples/AudioSeparator.Console` | Minimal console usage |
| `Examples/AudioSeparator.Spectre` | Progress bars with Spectre.Console |
| `Examples/AudioSeparator.Benchmark` | Performance benchmarking |

---

## Architecture details

See [`.cursor/skills/audio-separator/SKILL.md`](.cursor/skills/audio-separator/SKILL.md) for layer map, session flow, and authoring conventions.

---

## License

MIT — see [LICENSE](LICENSE) if present in repository root.
