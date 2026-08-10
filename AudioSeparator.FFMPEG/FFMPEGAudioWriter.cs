using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.FFMPEG;

public class FFMPEGAudioWriter(string ffmpegPath) : IAudioWriter
{
    public async Task WriteAsync(Stream destination, AudioChunk[] chunks, ModelMetadata modelMetadata, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-f f32le -ar {modelMetadata.AudioFrequency} -ac {modelMetadata.OutputChannels} -i pipe:0 -f mp3 -c:a libmp3lame pipe:1",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to open ffmpeg process");

        var errTask = process.StandardError.ReadToEndAsync();

        var writeTask = Task.Run(async () =>
        {
            try
            {
                foreach (var chunk in chunks.OrderBy(c => c.Index))
                {
                    var span = chunk.Samples.Span;

                    //TODO: Report Progress
                    var buffer = MemoryMarshal.AsBytes(chunk.Samples.Span).ToArray();

                    await process.StandardInput.BaseStream.WriteAsync(buffer, cancellationToken);
                }

                await process.StandardInput.BaseStream.FlushAsync(cancellationToken);
            }
            catch (IOException) when (!process.HasExited)
            {
                throw;
            }
            catch (IOException) when (process.ExitCode is 0)
            {
                
            }
            finally
            {
                process.StandardInput.Close();
            }
        });

        var readTask = Task.Run(async () =>
        {
            var readed = 0;
            var buffer = new byte[8192];

            while (true)
            {
                readed = await process.StandardOutput.BaseStream.ReadAsync(buffer, cancellationToken);
                if (readed is 0)
                {
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, readed));
            }
        });

        await Task.WhenAll(writeTask, readTask).WaitAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stdErr = await errTask;

        Console.WriteLine(stdErr);
        Console.WriteLine($"FFmpeg exit code: {process.ExitCode}");
    }

    public async Task WriteAsync(string fileName, AudioChunk[] chunks, ModelMetadata modelMetadata, CancellationToken cancellationToken = default)
    {
        using var fileStream = File.Open(fileName, FileMode.Create);

        await WriteAsync(fileStream, chunks, modelMetadata, cancellationToken);
    }
}