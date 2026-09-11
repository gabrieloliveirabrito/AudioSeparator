namespace AudioSeparator.Abstractions;

/// <summary>
/// Processing options applied during separation. Overlap-add increases inference time and CPU/GPU usage.
/// </summary>
public sealed class SeparationProcessingOptions
{
    public bool EnableOverlapAdd { get; set; }

    public float OverlapRatio { get; set; } = 0.25f;

    /// <summary>
    /// When set, only this stem is materialized in <see cref="SeparationResult"/>.
    /// </summary>
    public string? OutputStemName { get; set; }

    /// <summary>
    /// When true and <see cref="SeparationRequirements.SampleRate"/> is set, resample
    /// source PCM in Core to the required rate instead of failing validation.
    /// </summary>
    public bool EnableResample { get; set; } = true;

    /// <summary>
    /// Peak-normalize input before inference and restore scale on output stems.
    /// </summary>
    public bool EnablePeakNormalize { get; set; }

    /// <summary>
    /// Applied to materialized stems after inference (and after peak restore).
    /// </summary>
    public ClipPreventMode ClipPreventMode { get; set; } = ClipPreventMode.Rescale;

    public void Validate()
    {
        if (!EnableOverlapAdd)
        {
            return;
        }

        if (OverlapRatio is <= 0f or >= 0.5f)
        {
            throw new InvalidOperationException(
                $"Overlap ratio must be in (0, 0.5) when overlap-add is enabled, but was {OverlapRatio}.");
        }
    }
}
