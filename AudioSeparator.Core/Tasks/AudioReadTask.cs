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

        var chunks = new List<AudioChunk>();
        if (!string.IsNullOrEmpty(context.InputFilename))
        {
            await foreach (var chunk in context.AudioReader.ReadAsync(
                context.InputFilename,
                context.InferenceSpec.InputFrameCount,
                cancellationToken))
            {
                chunks.Add(chunk);
            }
        }
        else
        {
            context.InputStream.ThrowIfNull();
            await foreach (var chunk in context.AudioReader.ReadAsync(
                context.InputStream,
                context.InferenceSpec.InputFrameCount,
                cancellationToken))
            {
                chunks.Add(chunk);
            }
        }

        context.InputChunks = chunks.ToArray().AsMemory();
        ReportProgress(100, 100);
    }
}
