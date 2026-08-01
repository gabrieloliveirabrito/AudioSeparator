namespace AudioSeparator.Core;

public sealed class AudioSeparatorContext(AudioSeparatorBase separator)
{
    public AudioSeparatorBase Separator { get; } = separator;
}