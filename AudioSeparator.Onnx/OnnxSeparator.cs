using AudioSeparator.Core;

namespace AudioSeparator.Onnx;

public abstract class OnnxSeparator<TContext>(OnnxSeparatorBuilderContext builderContext) : AudioSeparatorBase<TContext>(builderContext)
where TContext : OnnxContext
{
    protected override TContext CreateContext()
    {
        return (TContext)new OnnxContext(builderContext);
    }

    public override void Dispose()
    {
        base.Dispose();

        builderContext.SessionOptions.Dispose();
    }
}