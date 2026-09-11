using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Core.Audio;

namespace AudioSeparator.Core.Tasks;

public class AudioPrepareTask(AudioSeparatorContext context) : ProcessTask("Preparing audio")
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        context.SourceInfo.ThrowIfNull();

        if (context.InputSamples.Length is 0)
        {
            throw new InvalidOperationException("Input samples are not available in memory.");
        }

        ReportProgress(0, 100);

        var options = context.ProcessingOptions;
        var source = context.SourceInfo;
        var samples = context.InputSamples;
        var channels = source.Channels;

        var targetRate = context.Requirements.SampleRate;
        if (options.EnableResample && targetRate > 0 && source.SampleRate != targetRate)
        {
            cancellationToken.ThrowIfCancellationRequested();
            samples = LinearResampler.Resample(samples, channels, source.SampleRate, targetRate);
            var frameCount = samples.Length / channels;
            context.SourceInfo = new AudioSourceInfo
            {
                SampleRate = targetRate,
                SampleCount = frameCount,
                Channels = channels,
                ChunkCount = source.ChunkCount
            };
            source = context.SourceInfo;
        }

        context.InputPeakScale = 1f;
        if (options.EnablePeakNormalize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var peak = PeakNormalizer.FindPeak(samples);
            if (peak > 0f)
            {
                PeakNormalizer.ScaleInPlace(samples, 1f / peak);
                context.InputPeakScale = peak;
            }
        }

        context.InputSamples = samples;
        ReportProgress(100, 100);
        return Task.CompletedTask;
    }
}
