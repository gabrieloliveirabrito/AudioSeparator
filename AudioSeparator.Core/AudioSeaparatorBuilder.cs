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
    public abstract IAudioSeparator Build(string modelPath);

    protected virtual TBuilder CastThis()
    {
        return (TBuilder)this;
    }

    public virtual TBuilder UseAudio(IAudioReader reader, IAudioWriter writer)
    {
        Context.AudioReader = reader;
        Context.AudioWriter = writer;

        return CastThis();
    }
}