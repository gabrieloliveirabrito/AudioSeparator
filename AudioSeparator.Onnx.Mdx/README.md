# AudioSeparator.Onnx.Mdx

> MDX-Net ONNX backend for AudioSeparator — UVR-style instrumental/vocals separation.

[![NuGet](https://img.shields.io/nuget/v/AudioSeparator.Onnx.Mdx)](https://www.nuget.org/packages/AudioSeparator.Onnx.Mdx)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

**Part of [AudioSeparator](https://github.com/gabrieloliveirabrito/AudioSeparator)** — modular .NET audio stem separation.

---

## Install

```bash
dotnet add package AudioSeparator.Onnx.Mdx
dotnet add package AudioSeparator.NAudio
```

---

## Defaults (UVR-MDX-NET-Inst_HQ_5)

| Setting | Default |
|---------|---------|
| Sample rate | 44100 Hz |
| Stems | `instrumental`, `vocals` |
| n_fft / hop / dim_f / dim_t | 7680 / 1024 / 3072 / 256 |
| Compensate | 1.021 |
| Overlap-add | enabled (0.25) |
| Peak normalize | enabled |
| Clip prevention | Rescale |

The ONNX graph receives spectrograms; STFT/iSTFT runs in this package.

---

## Quick start

```csharp
using AudioSeparator.NAudio;
using AudioSeparator.Onnx.Mdx;
using Microsoft.ML.OnnxRuntime;

using var separator = MdxBuilder.Create("UVR-MDX-NET-Inst_HQ_5.onnx")
    .UseNAudio()
    .WithOutputStem("instrumental")
    .ConfigureSessionOptions(o =>
    {
        o.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        o.AppendExecutionProvider_CUDA(0);
    })
    .Build();

var session = await separator.CreateSession("input.wav");
using var result = await session.RunAsync();
await result.WriteToDirectoryAsync("./Outputs");
```

---

## License

MIT — see [AudioSeparator repository](https://github.com/gabrieloliveirabrito/AudioSeparator).
