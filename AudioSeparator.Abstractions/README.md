# AudioSeparator.Abstractions

> Contracts and DTOs for modular audio stem separation — separators, sessions, I/O abstractions, and separation results.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.Abstractions)](https://www.nuget.org/packages/AudioSeparator.Abstractions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.Abstractions
```

> Most applications also reference a backend (e.g. [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs)) and an I/O package ([NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio) or [FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG)).

---

## When to use this package

- You are **authoring** a custom separator backend or I/O plugin and need the shared contracts.
- You need the **result types** (`SeparationResult`, `StemAudio`) and write extensions without pulling in ONNX or file I/O implementations.
- You are building benchmark tooling that observes any `IAudioSeparator`.

You typically **do not** reference only this package in an end-user app — pair it with a concrete backend and reader/writer.

---

## Core principle

> The separator **returns** separated stems as `SeparationResult`. It does **not** write files. Persistence is handled by I/O extensions (`WriteToDirectoryAsync`) using a registered `IAudioWriter`.

---

## Quick start

```csharp
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;

// After separation (from any IAudioSeparator implementation):
SeparationResult result = await session.RunAsync();

// Write stems when a writer was registered at build time:
await result.WriteToDirectoryAsync("./Outputs");

// Or write a single stem with an explicit writer:
await result.Stems["vocals"].WriteToFileAsync("./vocals.wav", writer);
```

---

## Session flow

| Step | Action | Output |
|------|--------|--------|
| 1 | `CreateSession(inputPath)` | Probe + validation |
| 2 | Attach progress on `session.Tasks` | UI feedback |
| 3 | `RunAsync()` | `SeparationResult` |
| 4 | `WriteToDirectoryAsync(directory)` | Files on disk |

---

## Key API

| Type | Role |
|------|------|
| `IAudioSeparator` | Factory for separation sessions |
| `ISeparationSession` | Runs the pipeline; exposes `Tasks` for progress |
| `IAudioSeparatorContext` | Runtime context shared across tasks |
| `IAudioSeparatorBuilder<TBuilder>` | Fluent builder: `UseAudio`, `UseStemNames`, `WithRequirements` |
| `IAudioReader` | Probe and read input audio (`ProbeAsync`, `ReadAsync`) |
| `IAudioWriter` | Write a `StemAudio` to disk |
| `AudioSourceInfo` | Probe result (sample rate, channels, duration) |
| `AudioChunk` | Chunk of float samples from the reader |
| `SeparationRequirements` | Builder config: sample rate, stem names |
| `InferenceSpec` | ONNX tensor layout (names, dims, stems — no sample rate) |
| `SeparationResult` | Output: stems dictionary + writer reference |
| `StemAudio` | One separated stem (samples, sample rate, channels) |
| `IProcessTask` | Pipeline task with progress callbacks |
| `SeparationResultExtensions` | `WriteToDirectoryAsync` |
| `StemAudioExtensions` | `WriteToFileAsync` |
| `ISeparationBenchmarkObserver` | Per-phase benchmark hooks |
| `SeparationBenchmarkOptions` | Warmup, probe, output directory |
| `SeparationBenchmarkReport` | Timing, memory, RTF metrics |
| `SeparationBenchmarkPhase` | `Probe`, `AudioRead`, `Inference`, `ResultAssembly`, `StemWrite` |

---

## Requirements

- **.NET 10**
- **Zero NuGet dependencies** — this package is contracts only

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.Core](https://www.nuget.org/packages/AudioSeparator.Core) | Pipeline implementation | Session API and tasks |
| [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | Demucs backend | Recommended for end users |
| [AudioSeparator.NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio) | NAudio I/O | WAV/MP3 read, WAV write |
| [AudioSeparator.FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) | FFMPEG I/O | Broad format support |
| [AudioSeparator.Benchmark](https://www.nuget.org/packages/AudioSeparator.Benchmark) | Benchmarking | Performance measurement |

Source: [AudioSeparator.Abstractions on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.Abstractions)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
