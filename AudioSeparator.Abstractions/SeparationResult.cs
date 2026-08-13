using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions;

public sealed class SeparationResult
{
    public required AudioSourceInfo Source { get; init; }
    public required IReadOnlyDictionary<string, StemAudio> Stems { get; init; }
    public required IAudioWriter Writer { get; init; }
}
