using AudioSeparator.Core;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx;

public class OnnxSeparatorBuilderContext : AudioSeparatorBuilderContext
{
    public string ModelPath { get; set; } = string.Empty;
    public Action<SessionOptions>? ConfigureSession { get; set; }
}
