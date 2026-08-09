using AudioSeparator.Abstractions.Builder;

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

    public static TBuilder UseFFMPEG<TBuilder>(this IAudioSeparatorBuilder<TBuilder> builder)
    where TBuilder : IAudioSeparatorBuilder<TBuilder>
    {
        var ffmpegPath = FindExecutablePath("ffmpeg");
        var ffprobePath = FindExecutablePath("ffprobe");

        return builder.UseAudio(new FFMPEGAudioReader(ffmpegPath, ffprobePath), new FFMPEGAudioWriter(ffmpegPath));
    }
}
