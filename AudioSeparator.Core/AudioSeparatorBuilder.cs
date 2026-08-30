namespace AudioSeparator.Core;

using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Builder;

public abstract class AudioSeparatorBuilder<TBuilder, TContext> : IAudioSeparatorBuilder<TBuilder>
where TBuilder : AudioSeparatorBuilder<TBuilder, TContext>
where TContext : AudioSeparatorBuilderContext
{
    protected TContext Context { get; set; }

    protected AudioSeparatorBuilder()
    {
        Context = CreateContext();
    }

    protected abstract TContext CreateContext();

    public abstract IAudioSeparator Build();

    protected virtual TBuilder CastThis() => (TBuilder)this;

    public virtual TBuilder UseAudio(IAudioReader reader, IAudioWriter writer)
    {
        Context.AudioReader = reader;
        Context.AudioWriter = writer;
        return CastThis();
    }

    public virtual TBuilder UseReader(IAudioReader reader)
    {
        Context.AudioReader = reader;
        return CastThis();
    }

    public virtual TBuilder UseStemNames(params string[] stemNames)
    {
        Context.Requirements.StemNames = stemNames;
        return CastThis();
    }

    public virtual TBuilder WithRequirements(SeparationRequirements requirements)
    {
        Context.Requirements = requirements;
        return CastThis();
    }

    public virtual TBuilder WithProcessingOptions(SeparationProcessingOptions options)
    {
        Context.ProcessingOptions = options;
        return CastThis();
    }

    public virtual TBuilder WithOutputStem(string stemName)
    {
        Context.ProcessingOptions.OutputStemName = stemName;
        return CastThis();
    }

    public virtual TBuilder WithOverlapAdd(bool enabled = true, float overlapRatio = 0.25f)
    {
        Context.ProcessingOptions.EnableOverlapAdd = enabled;
        Context.ProcessingOptions.OverlapRatio = overlapRatio;
        return CastThis();
    }
}
