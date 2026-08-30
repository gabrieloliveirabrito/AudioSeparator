# AudioSeparator.FFMPEG

> FFMPEG/ffprobe extensions for AudioSeparator — broad format support for reading input and encoding separated stems.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.FFMPEG)](https://www.nuget.org/packages/AudioSeparator.FFMPEG)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.FFMPEG
```

Pair with a separator backend, e.g. [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs).

---

## When to use this package

- You need **broad input format** support (anything ffmpeg can decode).
- You want **configurable output** format and codec (WAV, MP3, etc.).
- You already have **ffmpeg** and **ffprobe** on the system.

For simple WAV/MP3 without ffmpeg, consider [AudioSeparator.NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio).

---

## Quick start

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

using var separator = builder.Build();
var session = await separator.CreateSession("input.flac");
var result = await session.RunAsync();

await result.WriteToDirectoryAsync("./Outputs");
```

---

## Executable resolution

`ffmpeg` and `ffprobe` are resolved via:

1. **`FFMPEG_PATH`** environment variable (directory containing both executables)
2. **`PATH`** — first match on each path entry

Use `FFMPEGExtensions.FindExecutablePath("ffmpeg")` to resolve paths manually.

---

## Key API

| Type | Role |
|------|------|
| `FFMPEGExtensions.UseFFMPEG<TBuilder>(configure?)` | Registers reader + writer on the builder |
| `FFMPEGExtensions.FindExecutablePath(executable)` | Resolves `ffmpeg` / `ffprobe` path |
| `FFMPEGAudioReader` | `IAudioReader` — ffprobe probe + ffmpeg pipe decode |
| `FFMPEGAudioWriter` | `IAudioWriter` — ffmpeg pipe encode |
| `FFMPEGSettings` | `OutputFormat` (default `wav`), `OutputCodec` (default `pcm_s16le`) |
| `FFProbeResult` | ffprobe JSON DTOs for probe metadata |

---

## Requirements

- **.NET 10**
- [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions)
- **ffmpeg** and **ffprobe** on `PATH` or in `FFMPEG_PATH`

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | Demucs backend | Recommended separator |
| [AudioSeparator.NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio) | NAudio I/O | Simpler I/O without ffmpeg |
| [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions) | Contracts | `IAudioReader` / `IAudioWriter` |

Source: [AudioSeparator.FFMPEG on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.FFMPEG)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
