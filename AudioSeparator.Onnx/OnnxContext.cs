using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Core;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx;

public class OnnxContext : AudioSeparatorContext
{
    public OnnxContext(OnnxSeparatorBuilderContext builderContext, InferenceSession session, InferenceSpec inferenceSpec)
        : base(builderContext)
    {
        Session = session;
        InferenceSpec = inferenceSpec;
        DisposableResource = session;
    }

    public InferenceSession Session { get; }
}
