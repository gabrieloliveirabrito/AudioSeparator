using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;

namespace AudioSeparator.Core.Tasks;

public class AudioReadTask(AudioSeparatorContext context) : ProcessTask("Reading audio file")
{
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        context.ModelMetadata.ThrowIfNull();
        //context.AudioMetadata.ThrowIfNull();

        ReportProgress(0, 100);

        using var inputStream = string.IsNullOrEmpty(context.InputFilename) ? context.InputStream : File.OpenRead(context.InputFilename);
        inputStream.ThrowIfNull();

        var chunks = new List<AudioChunk>();
        await foreach (var chunk in context.AudioReader.ReadAsync(inputStream, context.ModelMetadata.InputSize, cancellationToken))
        {
            chunks.Add(chunk);
        }
        context.InputChunks = chunks.ToArray().AsMemory();

        ReportProgress(100, 100);
    }
}