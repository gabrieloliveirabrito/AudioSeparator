using System.Diagnostics;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.FFMPEG;

public class FFMPEGAudioWriter(string ffmpegPath, Entities.FFMPEGSettings settings) : IAudioWriter
{
    public string PreferredExtension => settings.OutputFormat;

    public async Task WriteAsync(Stream destination, StemAudio stem, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-f f32le -ar {stem.SampleRate} -ac {stem.Channels} -i pipe:0 -f {settings.OutputFormat} -c:a {settings.OutputCodec} pipe:1",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffmpeg process");

        var errTask = process.StandardError.ReadToEndAsync(cancellationToken);

        var writeTask = Task.Run(async () =>
        {
            try
            {
                await StemAudioStream.CopyPcmToAsync(
                    stem.Audio,
                    process.StandardInput.BaseStream,
                    cancellationToken);
                await process.StandardInput.BaseStream.FlushAsync(cancellationToken);
            }
            finally
            {
                process.StandardInput.Close();
            }
        }, cancellationToken);

        var readTask = Task.Run(async () =>
        {
            var buffer = new byte[8192];
            while (true)
            {
                var read = await process.StandardOutput.BaseStream.ReadAsync(buffer, cancellationToken);
                if (read is 0)
                {
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }, cancellationToken);

        await Task.WhenAll(writeTask, readTask).WaitAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stderr = await errTask;
        if (process.ExitCode != 0)
        {
            throw new Exception(string.IsNullOrEmpty(stderr)
                ? $"ffmpeg exited with code {process.ExitCode}"
                : stderr);
        }
    }

    public async Task WriteAsync(string fileName, StemAudio stem, CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.Create(fileName);
        await WriteAsync(fileStream, stem, cancellationToken);
    }
}
