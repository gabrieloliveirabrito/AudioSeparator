using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Core;

public abstract class AudioSeparatorBase<TContext>(AudioSeparatorBuilderContext builderContext) : IAudioSeparator
where TContext : AudioSeparatorContext
{
    private bool _disposing;

    protected AudioSeparatorBuilderContext BuilderContext { get; } = builderContext;

    protected virtual TContext CreateContext()
    {
        return (TContext)new AudioSeparatorContext(BuilderContext);
    }

    protected abstract IEnumerable<IProcessTask> CreateProcessesTask(TContext context);

    protected virtual void ValidateSource(AudioSeparatorContext context, AudioSourceInfo source)
    {
        var requirements = context.Requirements;

        if (source.SampleRate != requirements.SampleRate)
        {
            throw new InvalidOperationException(
                $"Expected sample rate {requirements.SampleRate} Hz, but the source is {source.SampleRate} Hz.");
        }

        var expectedChannels = context.InferenceSpec?.InputChannels;
        if (expectedChannels is > 0 && source.Channels != expectedChannels)
        {
            throw new InvalidOperationException(
                $"Expected {expectedChannels} channels, but the source has {source.Channels}.");
        }
    }

    public virtual async Task<ISeparationSession> CreateSession(
        string inputPath,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext();
        context.InputFilename = inputPath;
        context.InferenceSpec.ThrowIfNull();

        var source = await context.AudioReader.ProbeAsync(
            inputPath,
            context.InferenceSpec.InputFrameCount,
            cancellationToken);

        context.SourceInfo = source;
        ValidateSource(context, source);

        var tasks = CreateProcessesTask(context).ToList();
        return new SeparationSession(context, tasks, source);
    }

    public virtual void Dispose()
    {
        if (!_disposing)
        {
            _disposing = true;
        }
    }
}
