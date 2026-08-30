using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions;

public static class SeparationResultExtensions
{
    public static Task WriteToDirectoryAsync(
        this SeparationResult result,
        string directory,
        CancellationToken cancellationToken = default)
    {
        if (result.Writer is null)
        {
            throw new InvalidOperationException(
                "A writer is required to write stems to disk. Configure one with UseAudio or UseNAudio/UseFFMPEG.");
        }

        return result.WriteToDirectoryAsync(directory, result.Writer, cancellationToken);
    }

    public static async Task WriteToDirectoryAsync(
        this SeparationResult result,
        string directory,
        IAudioWriter writer,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);

        foreach (var (name, stem) in result.Stems)
        {
            var extension = Path.GetExtension(name);
            var fileName = string.IsNullOrEmpty(extension)
                ? Path.Combine(directory, $"{name}.{writer.PreferredExtension}")
                : Path.Combine(directory, name);

            await writer.WriteAsync(fileName, stem, cancellationToken);
        }
    }

    /// <summary>
    /// Returns the raw PCM stream for a stem. Caller must not dispose it; owned by <paramref name="result"/>.
    /// </summary>
    public static Stream OpenStemPcmStream(this SeparationResult result, string stemName) =>
        result.Stems[stemName].OpenPcmStream();

    /// <summary>
    /// Encodes a stem via <see cref="SeparationResult.Writer"/> into a new stream (WAV, MP3, etc.).
    /// Caller owns and must dispose the returned stream.
    /// </summary>
    public static Task<Stream> OpenStemEncodedStreamAsync(
        this SeparationResult result,
        string stemName,
        CancellationToken cancellationToken = default)
    {
        if (result.Writer is null)
        {
            throw new InvalidOperationException(
                "A writer is required for encoded output. Configure one with UseNAudio or UseFFMPEG.");
        }

        return result.Stems[stemName].OpenEncodedStreamAsync(result.Writer, cancellationToken);
    }

    /// <summary>
    /// Copies raw IEEE float32 interleaved PCM (no WAV/MP3 header). Use a <c>.pcm</c> file extension.
    /// For playable files, use <see cref="OpenStemEncodedStreamAsync"/> or <see cref="WriteToDirectoryAsync"/>.
    /// </summary>
    public static Task CopyStemPcmToAsync(
        this SeparationResult result,
        string stemName,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        return result.Stems[stemName].CopyPcmToAsync(destination, cancellationToken);
    }

    public static Task WriteStemToFileAsync(
        this SeparationResult result,
        string stemName,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (result.Writer is null)
        {
            throw new InvalidOperationException(
                "A writer is required to write encoded audio. Configure one with UseNAudio or UseFFMPEG.");
        }

        return result.Stems[stemName].WriteToFileAsync(filePath, result.Writer, cancellationToken);
    }
}

public static class StemAudioExtensions
{
    /// <summary>
    /// Raw PCM IEEE float32 interleaved stream. Position is reset to 0.
    /// Do not dispose when the parent <see cref="SeparationResult"/> still owns it.
    /// </summary>
    public static Stream OpenPcmStream(this StemAudio stem)
    {
        StemAudioStream.ResetPosition(stem.Audio);
        return stem.Audio;
    }

    /// <summary>
    /// Encodes this stem via <paramref name="writer"/> into a new stream (WAV, MP3, etc.).
    /// Caller owns and must dispose the returned stream.
    /// </summary>
    public static async Task<Stream> OpenEncodedStreamAsync(
        this StemAudio stem,
        IAudioWriter writer,
        CancellationToken cancellationToken = default)
    {
        var encoded = new MemoryStream();
        StemAudioStream.ResetPosition(stem.Audio);
        await writer.WriteAsync(encoded, stem, cancellationToken);
        encoded.Position = 0;
        return encoded;
    }

    public static Task WriteToFileAsync(
        this StemAudio stem,
        string filePath,
        IAudioWriter writer,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return writer.WriteAsync(filePath, stem, cancellationToken);
    }

    /// <summary>
    /// Copies raw IEEE float32 interleaved PCM (no container header). Use a <c>.pcm</c> file extension.
    /// </summary>
    public static Task CopyPcmToAsync(
        this StemAudio stem,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        return StemAudioStream.CopyPcmToAsync(stem.Audio, destination, cancellationToken);
    }
}
