using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Inference;

namespace AudioSeparator.Abstractions;

public interface IAudioSeparatorContext
{
    IAudioReader AudioReader { get; set; }

    IAudioWriter? AudioWriter { get; set; }

    float[] InputSamples { get; set; }

    InferenceSpec? InferenceSpec { get; set; }

    AudioSourceInfo? SourceInfo { get; set; }

    SeparationRequirements Requirements { get; set; }

    SeparationProcessingOptions ProcessingOptions { get; set; }

    Dictionary<string, float[]> OutputStemSamples { get; set; }

    /// <summary>
    /// Peak absolute value of input before peak-normalize (1 when disabled).
    /// </summary>
    float InputPeakScale { get; set; }
}
