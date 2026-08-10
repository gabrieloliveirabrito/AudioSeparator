using AudioSeparator.Core;

namespace AudioSeparator.Onnx;

public abstract class OnnxSeparator<TContext>(string modelPath, OnnxBuilderContext context) : AudioSeparatorBase<TContext>(modelPath)
where TContext : OnnxContext
{
    public override void Dispose()
    {
        base.Dispose();

        context.SessionOptions.Dispose();
    }
}