using System.Collections.Concurrent;
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
        builderContext.AudioWriter.ThrowIfNull();
        AudioReader = builderContext.AudioReader;
        AudioWriter = builderContext.AudioWriter;
        Requirements = builderContext.Requirements;
    }

    public string? InputFilename { get; set; }
    public Stream? InputStream { get; set; }

    public IAudioReader AudioReader { get; set; }
    public IAudioWriter AudioWriter { get; set; }
    public InferenceSpec? InferenceSpec { get; set; }
    public AudioSourceInfo? SourceInfo { get; set; }
    public SeparationRequirements Requirements { get; set; }

    public Memory<AudioChunk> InputChunks { get; set; }
    public ConcurrentDictionary<string, AudioChunk[]> OutputStems { get; set; } = [];
    public IDisposable? DisposableResource { get; set; }
}
