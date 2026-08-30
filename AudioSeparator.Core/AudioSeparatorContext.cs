using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Inference;

namespace AudioSeparator.Core;

public class AudioSeparatorContext : IAudioSeparatorContext
{
    public AudioSeparatorContext(AudioSeparatorBuilderContext builderContext)
    {
        builderContext.AudioReader.ThrowIfNull();
        AudioReader = builderContext.AudioReader;
        AudioWriter = builderContext.AudioWriter;
        Requirements = builderContext.Requirements;
        ProcessingOptions = builderContext.ProcessingOptions;
    }

    public string? InputFilename { get; set; }

    public Stream? InputStream { get; set; }

    public IAudioReader AudioReader { get; set; }

    public IAudioWriter? AudioWriter { get; set; }

    public InferenceSpec? InferenceSpec { get; set; }

    public AudioSourceInfo? SourceInfo { get; set; }

    public SeparationRequirements Requirements { get; set; }

    public SeparationProcessingOptions ProcessingOptions { get; set; }

    public float[] InputSamples { get; set; } = [];

    public Dictionary<string, float[]> OutputStemSamples { get; set; } = [];

    public IDisposable? DisposableResource { get; set; }
}
