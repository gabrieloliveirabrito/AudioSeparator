namespace AudioSeparator.Abstractions;

public sealed class StemAudio
{
    public required string Name { get; init; }

    public required int SampleRate { get; init; }

    public required int Channels { get; init; }

    /// <summary>
    /// Raw PCM IEEE float32 interleaved samples. Position is 0 after <see cref="ISeparationSession.RunAsync"/>.
    /// Owned by <see cref="SeparationResult"/>; use <see cref="StemAudioExtensions.OpenPcmStream"/> before reading.
    /// For WAV/MP3 bytes, use <see cref="StemAudioExtensions.OpenEncodedStreamAsync"/>.
    /// </summary>
    public required Stream Audio { get; init; }
}
