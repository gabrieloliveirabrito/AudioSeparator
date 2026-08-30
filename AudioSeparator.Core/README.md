# AudioSeparator.Core

> Session API and task pipeline for AudioSeparator — probe, validate, run tasks, and return separated stems without file I/O.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.Core)](https://www.nuget.org/packages/AudioSeparator.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.Core
```

> Reference this package when **building a custom separator backend**. For Demucs/htdemucs, use [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) instead.

---

## When to use this package

- You are implementing a **non-ONNX** or custom pipeline backend on top of the shared session model.
- You need `AudioSeparatorBase`, `SeparationSession`, and `ProcessTask` to orchestrate read → infer → result assembly.
- You want the core pipeline **without** ONNX Runtime or model-specific code.

For ONNX backends, start from [AudioSeparator.Onnx](https://www.nuget.org/packages/AudioSeparator.Onnx) instead of subclassing Core directly.

---

## Core principle

> Core **returns** `SeparationResult` with materialized `StemAudio` per stem. There is no write task in the pipeline — file output lives in NAudio/FFMPEG extensions.

---

## Quick start

```csharp
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Core;

public sealed class MySeparator(MyBuilderContext context) : AudioSeparatorBase<MyContext>(context)
{
    protected override IEnumerable<IProcessTask> CreateProcessesTask(MyContext context)
    {
        yield return new AudioReadTask(context);
        // Add your inference task here
    }
}
```

Register reader/writer via the builder (`UseAudio`), then:

```csharp
using var separator = myBuilder.Build();
var session = await separator.CreateSession("input.wav");
var result = await session.RunAsync();
```

---

## Session flow

| Step | Component | What happens |
|------|-----------|--------------|
| 1 | `AudioSeparatorBase.CreateSession` | Probe source via `IAudioReader`, validate against requirements |
| 2 | `SeparationSession` | Runs `IProcessTask` list sequentially |
| 3 | `AudioReadTask` | Single read pass into input chunks |
| 4 | Inference task(s) | Backend-specific (e.g. ONNX in sibling package) |
| 5 | `RunAsync` | Returns `SeparationResult` with `StemAudio` per stem |

---

## Key API

| Type | Role |
|------|------|
| `AudioSeparatorBase<TContext>` | Abstract separator; creates session, validates source, wires tasks |
| `AudioSeparatorBuilder<TBuilder, TContext>` | Abstract fluent builder |
| `AudioSeparatorBuilderContext` | Builder state: reader, writer, requirements |
| `AudioSeparatorContext` | Runtime context (`Requirements`, `InferenceSpec`, chunks) |
| `SeparationSession` | `ISeparationSession` — runs tasks, assembles result |
| `ProcessTask` | Abstract task base with progress callbacks |
| `AudioReadTask` | Reads input via `IAudioReader` into `InputChunks` |

---

## Requirements

- **.NET 10**
- [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions)

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions) | Contracts | Interfaces and DTOs |
| [AudioSeparator.Onnx](https://www.nuget.org/packages/AudioSeparator.Onnx) | ONNX layer | Generic ONNX backend base |
| [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | Demucs | Ready-to-use Demucs backend |

Source: [AudioSeparator.Core on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.Core)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
