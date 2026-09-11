using AudioSeparator.Core.Audio;

namespace AudioSeparator.Core.Tasks;

public class AudioPostprocessTask(AudioSeparatorContext context) : ProcessTask("Post-processing stems")
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ReportProgress(0, 100);

        var peak = context.InputPeakScale;
        if (context.ProcessingOptions.EnablePeakNormalize && peak is not 0f and not 1f)
        {
            foreach (var samples in context.OutputStemSamples.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                PeakNormalizer.ScaleInPlace(samples, peak);
            }
        }

        var mode = context.ProcessingOptions.ClipPreventMode;
        if (mode != Abstractions.ClipPreventMode.None)
        {
            foreach (var samples in context.OutputStemSamples.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ClipPrevention.ApplyInPlace(samples, mode);
            }
        }

        ReportProgress(100, 100);
        return Task.CompletedTask;
    }
}
