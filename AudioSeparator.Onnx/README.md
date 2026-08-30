# AudioSeparator.Onnx

> ONNX Runtime backend for AudioSeparator — session lifecycle, builder hooks, and configurable execution providers.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.Onnx)](https://www.nuget.org/packages/AudioSeparator.Onnx)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.Onnx
```

> For Demucs/htdemucs models, use [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) — a ready-made wrapper around this package.

---

## When to use this package

- You are **authoring a custom ONNX model backend** (not Demucs).
- You need `OnnxSeparator`, `OnnxContext`, and `ConfigureSessionOptions`.
- You implement model-specific spec reading and inference tasks via abstract hooks.

End users separating audio with Demucs should prefer **Onnx.Demucs** over building on this layer directly.

---

## Core principle

> Sample rate comes from `SeparationRequirements` (builder config), **not** from the ONNX model metadata. `InferenceSpec` describes tensor layout only.

---

## Quick start

```csharp
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Onnx;
using Microsoft.ML.OnnxRuntime;

public sealed class MyOnnxSeparator(MyOnnxBuilderContext context) : OnnxSeparator<MyOnnxContext>(context)
{
    protected override InferenceSpec ReadInferenceSpec(InferenceSession session)
        => MyInferenceSpecReader.Read(session);

    protected override IProcessTask CreateInferenceTask(MyOnnxContext context)
        => new MyInferenceTask(context);
}

public sealed class MyOnnxBuilder(string modelPath) : OnnxSeparatorBuilder<MyOnnxBuilder, MyOnnxBuilderContext>
{
    public static MyOnnxBuilder Create(string path) => new(path);

    protected override MyOnnxBuilderContext CreateContext() => new();

    public override IAudioSeparator Build() => new MyOnnxSeparator(Context);

    private MyOnnxBuilder(string path)
    {
        Context.ModelPath = path;
    }
}

var separator = MyOnnxBuilder.Create("model.onnx")
    .UseStemNames("stem_a", "stem_b")
    .ConfigureSessionOptions(options =>
    {
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        options.AppendExecutionProvider_CUDA(0);
    })
    .Build();
```

---

## Pipeline

| Step | Component | What happens |
|------|-----------|--------------|
| 1 | `OnnxSeparator.CreateContext` | Load `InferenceSession`, call `ReadInferenceSpec` |
| 2 | `AudioReadTask` | Decode input into chunks |
| 3 | `CreateInferenceTask` | Model-specific chunked inference (implemented by subclass) |
| 4 | `RunAsync` | `SeparationResult` with per-stem `StemAudio` |

---

## Key API

| Type | Role |
|------|------|
| `OnnxSeparator<TContext>` | Extends `AudioSeparatorBase`; abstract hooks for spec + inference |
| `OnnxSeparatorBuilder<TBuilder, TContext>` | Adds `ConfigureSessionOptions(Action<SessionOptions>)` |
| `OnnxSeparatorBuilderContext` | `ModelPath`, `ConfigureSession` callback |
| `OnnxContext` | Holds `InferenceSession`; sets disposable resource |
| `ReadInferenceSpec(session)` | **Abstract** — map ONNX metadata → `InferenceSpec` |
| `CreateInferenceTask(context)` | **Abstract** — return model-specific inference task |

---

## Requirements

- **.NET 10**
- [AudioSeparator.Core](https://www.nuget.org/packages/AudioSeparator.Core) (transitive: Abstractions)
- **Microsoft.ML.OnnxRuntime.Gpu** 1.28.0 (included)
- **CUDA** (optional) — for GPU execution via `ConfigureSessionOptions`

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | Demucs backend | Recommended entry point |
| [AudioSeparator.Core](https://www.nuget.org/packages/AudioSeparator.Core) | Pipeline | Session and tasks |
| [AudioSeparator.NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio) | NAudio I/O | Reader/writer extensions |
| [AudioSeparator.FFMPEG](https://www.nuget.org/packages/AudioSeparator.FFMPEG) | FFMPEG I/O | Reader/writer extensions |

Source: [AudioSeparator.Onnx on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.Onnx)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
