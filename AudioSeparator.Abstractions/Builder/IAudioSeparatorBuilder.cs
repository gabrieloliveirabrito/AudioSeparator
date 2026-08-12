using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions.Builder;

public interface IAudioSeparatorBuilder<TBuilder>
where TBuilder : IAudioSeparatorBuilder<TBuilder>
{
    TBuilder UseReader(IAudioReader reader);
    TBuilder UseStemNames(params string[] stemNames);
    TBuilder WithRequirements(SeparationRequirements requirements);
    IAudioSeparator Build();
}
