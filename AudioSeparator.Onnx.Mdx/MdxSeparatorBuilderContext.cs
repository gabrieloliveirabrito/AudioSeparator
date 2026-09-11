using AudioSeparator.Onnx;

namespace AudioSeparator.Onnx.Mdx;

public class MdxSeparatorBuilderContext : OnnxSeparatorBuilderContext
{
    public MdxModelParams ModelParams { get; set; } = MdxModelParams.CreateInstHq5Defaults();
}
