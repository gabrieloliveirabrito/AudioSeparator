using AudioSeparator.Onnx;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx.Demucs;

public class DemucsContext(
    OnnxSeparatorBuilderContext builderContext,
    InferenceSession session,
    Abstractions.Inference.InferenceSpec inferenceSpec)
    : OnnxContext(builderContext, session, inferenceSpec)
{
}
