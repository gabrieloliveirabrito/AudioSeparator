namespace AudioSeparator.Onnx.Demucs;

using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Onnx;
using AudioSeparator.Onnx.Demucs.Inference;
using AudioSeparator.Onnx.Demucs.Tasks;
using Microsoft.ML.OnnxRuntime;

public class DemucsSeparator(DemucsSeparatorBuilderContext context) : OnnxSeparator<DemucsContext>(context)
{
    protected override InferenceSpec ReadInferenceSpec(InferenceSession session)
        => DemucsInferenceSpecReader.Read(session);

    protected override IProcessTask CreateInferenceTask(DemucsContext context)
        => new DemucsInferenceTask(context);
}
