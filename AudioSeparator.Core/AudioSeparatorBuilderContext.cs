using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Builder;

namespace AudioSeparator.Core;

public abstract class AudioSeparatorBuilderContext : IAudioSeparatorBuilderContext
{
    public IAudioReader? AudioReader { get; set; }
    public IAudioWriter? AudioWriter { get; set; }

    public string[] StemNames { get; set; } = [];
}