using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.Abstractions.Builder;

public interface IAudioSeparatorBuilder<TBuilder>
where TBuilder : IAudioSeparatorBuilder<TBuilder>
{
    TBuilder UseAudio(IAudioReader reader, IAudioWriter writer);

    TBuilder UseReader(IAudioReader reader);

    TBuilder UseStemNames(params string[] stemNames);

    TBuilder WithRequirements(SeparationRequirements requirements);

    TBuilder WithProcessingOptions(SeparationProcessingOptions options);

    TBuilder WithOutputStem(string stemName);

    /// <summary>
    /// Enables overlap-add stitching. Increases inference time and CPU/GPU usage.
    /// </summary>
    TBuilder WithOverlapAdd(bool enabled = true, float overlapRatio = 0.25f);

    /// <summary>
    /// Resample source PCM in Core to <see cref="SeparationRequirements.SampleRate"/> when rates differ.
    /// </summary>
    TBuilder WithResample(bool enabled = true);

    /// <summary>
    /// Peak-normalize input before inference and restore scale on stems.
    /// </summary>
    TBuilder WithPeakNormalize(bool enabled = true);

    /// <summary>
    /// Apply clip prevention to output stems (Demucs-style prevent_clip).
    /// </summary>
    TBuilder WithClipPrevention(ClipPreventMode mode);

    IAudioSeparator Build();
}
