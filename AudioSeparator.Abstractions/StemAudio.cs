using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions;

public sealed class StemAudio
{
    public required string Name { get; init; }
    public required int SampleRate { get; init; }
    public required int Channels { get; init; }
    public required AudioChunk[] Chunks { get; init; }
}
