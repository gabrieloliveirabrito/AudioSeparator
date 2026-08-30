using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions.Builder;

public interface IAudioSeparatorBuilderContext
{
    IAudioReader? AudioReader { get; set; }

    IAudioWriter? AudioWriter { get; set; }

    SeparationRequirements Requirements { get; set; }

    SeparationProcessingOptions ProcessingOptions { get; set; }
}
