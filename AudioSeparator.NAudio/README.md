# AudioSeparator.NAudio

> NAudio reader and writer extensions for AudioSeparator — decode input and save stems as WAV without FFMPEG.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.NAudio)](https://www.nuget.org/packages/AudioSeparator.NAudio)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.NAudio
```

Pair with a separator backend, e.g. [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs).

---

## When to use this package

- You want **simple I/O** without installing ffmpeg.
- Input is **WAV or MP3** (via NAudio / Media Foundation).
- Output stems as **IEEE float WAV** (`PreferredExtension`: `wav`).

For exotic formats (FLAC, OGG, MP3 output, etc.), use [AudioSeparator.FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) instead.

---

## Quick start

```csharp
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;

var builder = DemucsBuilder.Create("htdemucs.onnx")
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseNAudio(); // registers NAudioReader + NAudioWriter

using var separator = builder.Build();
var session = await separator.CreateSession("input.mp3");
var result = await session.RunAsync();

await result.WriteToDirectoryAsync("./Outputs"); // writes *.wav stems
```

`UseNAudio()` registers both reader and writer on any `IAudioSeparatorBuilder<TBuilder>`.

---

## Key API

| Type | Role |
|------|------|
| `NAudioExtensions.UseNAudio<TBuilder>()` | Registers `NAudioReader` + `NAudioWriter` on the builder |
| `NAudioReader` | `IAudioReader` — probes and reads WAV/MP3 |
| `NAudioWriter` | `IAudioWriter` — writes IEEE float WAV |

---

## Requirements

- **.NET 10**
- [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions)
- **NAudio** 2.3.0 (included)
- No external tools required

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | Demucs backend | Recommended separator |
| [AudioSeparator.FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) | FFMPEG I/O | Alternative I/O layer |
| [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions) | Contracts | `IAudioReader` / `IAudioWriter` |

Source: [AudioSeparator.NAudio on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.NAudio)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
