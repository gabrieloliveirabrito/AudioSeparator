using AudioSeparator.Core;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx;

public class OnnxBuilderContext : AudioSeparatorBuilderContext
{
    public SessionOptions SessionOptions { get; set; } = new();
}