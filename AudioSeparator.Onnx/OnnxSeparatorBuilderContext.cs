using AudioSeparator.Core;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx;

public class OnnxSeparatorBuilderContext : AudioSeparatorBuilderContext
{
    public string ModelPath { get; set; } = default!;
    public SessionOptions SessionOptions { get; set; } = new();
}