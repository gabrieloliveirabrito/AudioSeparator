using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Core;
using AudioSeparator.Core.Tasks;
using AudioSeparator.Onnx.Tasks;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx;

public abstract class OnnxSeparator<TContext>(OnnxSeparatorBuilderContext builderContext) : AudioSeparatorBase<TContext>(builderContext)
where TContext : OnnxContext
{
    protected OnnxSeparatorBuilderContext OnnxBuilderContext { get; } = builderContext;

    protected override TContext CreateContext()
    {
        OnnxBuilderContext.ModelPath.ThrowIfNull();

        var options = new SessionOptions();
        OnnxBuilderContext.ConfigureSession?.Invoke(options);

        var session = new InferenceSession(OnnxBuilderContext.ModelPath, options);
        var inferenceSpec = InferenceSpecReader.Read(session);

        return (TContext)Activator.CreateInstance(
            typeof(TContext),
            OnnxBuilderContext,
            session,
            inferenceSpec)!;
    }

    protected override IEnumerable<IProcessTask> CreateProcessesTask(TContext context)
    {
        yield return new AudioReadTask(context);
        yield return new OnnxInferenceTask(context);
    }
}
