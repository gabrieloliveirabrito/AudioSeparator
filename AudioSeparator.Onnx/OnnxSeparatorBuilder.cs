namespace AudioSeparator.Onnx;

using AudioSeparator.Core;
using Microsoft.ML.OnnxRuntime;

public abstract class OnnxSeparatorBuilder<TBuilder, TContext> : AudioSeparatorBuilder<TBuilder, TContext>
where TBuilder : OnnxSeparatorBuilder<TBuilder, TContext>
where TContext : OnnxSeparatorBuilderContext
{
    public TBuilder ConfigureSessionOptions(Action<SessionOptions> configure)
    {
        Context.ConfigureSession = configure;
        return CastThis();
    }
}
