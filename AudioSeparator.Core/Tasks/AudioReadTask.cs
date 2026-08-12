using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Core.Tasks;

public class AudioReadTask(AudioSeparatorContext context) : ProcessTask("Reading audio file")
{
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        context.InferenceSpec.ThrowIfNull();

        ReportProgress(0, 100);

        using var inputStream = string.IsNullOrEmpty(context.InputFilename)
            ? context.InputStream
            : File.OpenRead(context.InputFilename);
        inputStream.ThrowIfNull();

        var chunks = new List<AudioChunk>();
        await foreach (var chunk in context.AudioReader.ReadAsync(
            inputStream,
            context.InferenceSpec.InputFrameCount,
            cancellationToken))
        {
            chunks.Add(chunk);
        }

        context.InputChunks = chunks.ToArray().AsMemory();
        ReportProgress(100, 100);
    }
}
