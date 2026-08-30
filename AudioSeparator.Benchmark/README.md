# AudioSeparator.Benchmark

> Benchmark runners and reporters for AudioSeparator — warmup runs, per-phase timing, memory snapshots, and JSON/console reports.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.Benchmark)](https://www.nuget.org/packages/AudioSeparator.Benchmark)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.Benchmark
```

Works with any `IAudioSeparator` — compose your separator (e.g. [Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) + [NAudio](https://www.nuget.org/packages/AudioSeparator.NAudio)) separately.

---

## When to use this package

- You need **repeatable performance measurements** for stem separation.
- You want per-phase breakdown: probe, read, inference, assembly, write.
- You want **console tables** or **JSON reports** with RTF and samples/sec.

---

## Quick start

```csharp
using AudioSeparator.Abstractions.Benchmark;
using AudioSeparator.Benchmark;
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Demucs;
using Microsoft.ML.OnnxRuntime;

var builder = DemucsBuilder.Create("htdemucs.onnx")
    .UseStemNames("drums", "bass", "other", "vocals")
    .UseNAudio()
    .ConfigureSessionOptions(options =>
    {
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        options.AppendExecutionProvider_CUDA(0);
    });

using var separator = builder.Build();

var options = new SeparationBenchmarkOptions
{
    OutputDirectory = "./BenchmarkOutputs",
    WarmupRuns = 1,
    IncludeProbe = true,
    ForceGcBetweenPhases = false
};

var report = await SeparationBenchmarkRunner.RunAsync(
    separator,
    "input.wav",
    options);

ConsoleBenchmarkReporter.Write(report);
// Or: JsonBenchmarkReporter.Write(report);
```

---

## Benchmark phases

| Phase | `SeparationBenchmarkPhase` | What is measured |
|-------|------------------------------|------------------|
| Probe | `Probe` | `CreateSession` (metadata + validation) |
| Read | `AudioRead` | Input decode |
| Inference | `Inference` | Model execution |
| Assembly | `ResultAssembly` | Building `SeparationResult` |
| Write | `StemWrite` | Optional stem file output |

Reports include duration, managed memory, working set, **real-time factor (RTF)**, and **samples/sec**.

---

## Key API

| Type | Role |
|------|------|
| `SeparationBenchmarkRunner.RunAsync` | Warmup + timed run with observer |
| `BenchmarkReportBuilder.Build` | Builds `SeparationBenchmarkReport` from phase measurements |
| `ConsoleBenchmarkReporter.Write` | Formatted phase table to console |
| `JsonBenchmarkReporter.Serialize` / `Write` | JSON output (camelCase) |
| `MemorySampler.Capture` | Managed + working-set snapshot |

Benchmark DTOs (`SeparationBenchmarkOptions`, `SeparationBenchmarkReport`, `PhaseMeasurement`, `ISeparationBenchmarkObserver`) live in [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions).

---

## Requirements

- **.NET 10**
- [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions)
- Any `IAudioSeparator` implementation (not included)

---

## Related packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AudioSeparator.Onnx.Demucs](https://www.nuget.org/packages/AudioSeparator.Onnx.Demucs) | Demucs backend | Common benchmark target |
| [AudioSeparator.Abstractions](https://www.nuget.org/packages/AudioSeparator.Abstractions) | Contracts | Benchmark DTOs and observer |

Example host: [Examples/AudioSeparator.Benchmark](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/Examples/AudioSeparator.Benchmark)

Source: [AudioSeparator.Benchmark on GitHub](https://github.com/gabrieloliveirabrito/AudioSeparator/tree/main/AudioSeparator.Benchmark)

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
