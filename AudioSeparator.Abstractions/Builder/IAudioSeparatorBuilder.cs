using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions.Builder;

public interface IAudioSeparatorBuilder<TBuilder>
where TBuilder : IAudioSeparatorBuilder<TBuilder>
{
    TBuilder UseAudio(IAudioReader reader, IAudioWriter writer);
    IAudioSeparator Build(string modelPath);
}