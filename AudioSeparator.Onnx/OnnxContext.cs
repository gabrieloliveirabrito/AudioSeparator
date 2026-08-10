using AudioSeparator.Core;

namespace AudioSeparator.Onnx;

public class OnnxContext : AudioSeparatorContext
{
    public OnnxContext(OnnxSeparatorBuilderContext builderContext) : base(builderContext)
    {
        ModelPath = builderContext.ModelPath;
    }

    public virtual string ModelPath { get; set; }
}