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
        context.SourceInfo.ThrowIfNull();

        ReportProgress(0, 100);

        var channels = context.SourceInfo.Channels;
        var samples = new List<float>();

        if (!string.IsNullOrEmpty(context.InputFilename))
        {
            await foreach (var chunk in context.AudioReader.ReadAsync(
                context.InputFilename,
                context.InferenceSpec.InputFrameCount,
                cancellationToken))
            {
                AppendChunk(samples, chunk, channels);
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
                AppendChunk(samples, chunk, channels);
            }
        }

        context.InputSamples = samples.ToArray();
        ReportProgress(100, 100);
    }

    private static void AppendChunk(List<float> samples, AudioChunk chunk, int channels)
    {
        var frameCount = chunk.Length;
        var span = chunk.Samples.Span;
        var sampleCount = frameCount * channels;

        for (var index = 0; index < sampleCount; index++)
        {
            samples.Add(span[index]);
        }
    }
}
