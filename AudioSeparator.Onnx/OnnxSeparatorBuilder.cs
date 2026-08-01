using AudioSeparator.Core;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx;

public abstract class OnnxSeparatorBuilder<TBuilder, TContext> : AudioSeparatorBuilder<TBuilder, TContext>
where TBuilder : OnnxSeparatorBuilder<TBuilder, TContext>
where TContext : OnnxBuilderContext
{

    protected OnnxSeparatorBuilder() : base()
    {
    }

    public TBuilder ConfigureSessionOptions(Action<SessionOptions> configure)
    {
        configure(Context.SessionOptions);

        return CastThis();
    }
}
