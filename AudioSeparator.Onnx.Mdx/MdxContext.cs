using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Onnx;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx.Mdx;

public class MdxContext(
    MdxSeparatorBuilderContext builderContext,
    InferenceSession session,
    InferenceSpec inferenceSpec)
    : OnnxContext(builderContext, session, inferenceSpec)
{
    public MdxModelParams ModelParams { get; } = builderContext.ModelParams;
}
