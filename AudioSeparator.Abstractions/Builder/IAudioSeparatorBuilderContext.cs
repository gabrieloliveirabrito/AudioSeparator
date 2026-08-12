using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions.Builder;

public interface IAudioSeparatorBuilderContext
{
    IAudioReader? AudioReader { get; set; }
    SeparationRequirements Requirements { get; set; }
}
