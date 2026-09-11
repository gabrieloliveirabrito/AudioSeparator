using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Onnx;
using AudioSeparator.Onnx.Mdx.Inference;
using AudioSeparator.Onnx.Mdx.Tasks;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx.Mdx;

public class MdxSeparator(MdxSeparatorBuilderContext context) : OnnxSeparator<MdxContext>(context)
{
    private readonly MdxSeparatorBuilderContext _mdxBuilderContext = context;

    protected override InferenceSpec ReadInferenceSpec(InferenceSession session)
        => MdxInferenceSpecReader.Read(session, _mdxBuilderContext.ModelParams);

    protected override IProcessTask CreateInferenceTask(MdxContext context)
        => new MdxInferenceTask(context);
}
