using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Builder;
using AudioSeparator.FFMPEG.Entities;

namespace AudioSeparator.FFMPEG;

public static class FFMPEGExtensions
{
    public static string FindExecutablePath(string executable)
    {
        if (OperatingSystem.IsWindows())
        {
            executable = $"{executable}.exe";
        }

        var envVar = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        if (!string.IsNullOrEmpty(envVar))
        {
            var filename = Path.Combine(envVar, executable);
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException($"The executable {executable} hasn't been found on path {filename}!");
            }

            return filename;
        }

        var pathString = Environment.GetEnvironmentVariable("PATH") ?? throw new InvalidOperationException("The PATH environment is null!");
        var paths = pathString.Split(Path.PathSeparator);

        foreach (var path in paths)
        {
            var filename = Path.Combine(path, executable);
            if (File.Exists(filename))
            {
                return filename;
            }
        }

        throw new FileNotFoundException($"The executable {executable} hasn't been found!");
    }

    public static TBuilder UseFFMPEG<TBuilder>(this IAudioSeparatorBuilder<TBuilder> builder, Action<FFMPEGSettings>? configure = null)
    where TBuilder : IAudioSeparatorBuilder<TBuilder>
    {
        var settings = new FFMPEGSettings();
        configure?.Invoke(settings);

        var ffmpegPath = FindExecutablePath("ffmpeg");
        var ffprobePath = FindExecutablePath("ffprobe");

        return builder.UseReader(new FFMPEGAudioReader(ffmpegPath, ffprobePath));
    }

    public static FFMPEGAudioWriter CreateWriter(Action<FFMPEGSettings>? configure = null)
    {
        var settings = new FFMPEGSettings();
        configure?.Invoke(settings);
        return new FFMPEGAudioWriter(FindExecutablePath("ffmpeg"), settings);
    }
}
