using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Core.Audio;

namespace AudioSeparator.Core;

public abstract class AudioSeparatorBase<TContext>(AudioSeparatorBuilderContext builderContext) : IAudioSeparator
where TContext : AudioSeparatorContext
{
    private bool _disposing;

    protected AudioSeparatorBuilderContext BuilderContext { get; } = builderContext;

    protected virtual TContext CreateContext() => (TContext)new AudioSeparatorContext(BuilderContext);

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

    public Task<ISeparationSession> CreateSession(Stream input, CancellationToken cancellationToken = default)
    {
        if (!input.CanSeek)
        {
            throw new InvalidOperationException(
                "Non-seekable streams require source metadata. Use CreateSession(stream, sourceInfo).");
        }

        input.Seek(0, SeekOrigin.Begin);
        return CreateSessionFromStreamAsync(input, sourceInfo: null, cancellationToken);
    }

    public Task<ISeparationSession> CreateSession(
        Stream input,
        AudioSourceInfo sourceInfo,
        CancellationToken cancellationToken = default) =>
        CreateSessionFromStreamAsync(input, sourceInfo, cancellationToken);

    private async Task<ISeparationSession> CreateSessionFromStreamAsync(
        Stream input,
        AudioSourceInfo? sourceInfo,
        CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.InputStream = input;
        context.InferenceSpec.ThrowIfNull();

        var source = sourceInfo ?? await context.AudioReader.ProbeAsync(
            input,
            context.InferenceSpec.InputFrameCount,
            cancellationToken);

        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }

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
