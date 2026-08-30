using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions;

public sealed class SeparationResult : IDisposable
{
  private bool _disposed;

    public required AudioSourceInfo Source { get; init; }

    public required IReadOnlyDictionary<string, StemAudio> Stems { get; init; }

    public IAudioWriter? Writer { get; init; }

    public Stream GetStemAudio(string stemName) => Stems[stemName].Audio;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var stem in Stems.Values)
        {
            stem.Audio.Dispose();
        }
    }
}
