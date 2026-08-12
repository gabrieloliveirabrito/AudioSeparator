using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Builder;

namespace AudioSeparator.NAudio;

public static class NAudioExtensions
{
    public static TBuilder UseNAudio<TBuilder>(this TBuilder builder)
    where TBuilder : IAudioSeparatorBuilder<TBuilder>
    {
        return builder.UseReader(new NAudioReader());
    }

    public static NAudioWriter CreateWriter()
    {
        return new NAudioWriter();
    }
}
