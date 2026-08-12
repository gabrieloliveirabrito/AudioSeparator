namespace AudioSeparator.Abstractions.Audio;

public sealed class AudioSourceInfo
{
    public int SampleRate { get; init; }
    public long SampleCount { get; init; }
    public int Channels { get; init; }
    public int ChunkCount { get; init; }
}
