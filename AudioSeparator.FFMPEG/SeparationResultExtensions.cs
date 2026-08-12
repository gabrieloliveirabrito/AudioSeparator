using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.FFMPEG;

public static class SeparationResultExtensions
{
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
                ? Path.Combine(directory, $"{name}.wav")
                : Path.Combine(directory, name);

            await writer.WriteAsync(fileName, stem, cancellationToken);
        }
    }
}

public static class StemAudioExtensions
{
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
}
