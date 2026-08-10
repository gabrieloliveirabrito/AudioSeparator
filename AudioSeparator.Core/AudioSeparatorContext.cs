using System.Collections.Concurrent;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Core;

public class AudioSeparatorContext : IAudioSeparatorContext
{
    public AudioSeparatorContext(AudioSeparatorBuilderContext builderContext)
    {
        builderContext.AudioWriter.ThrowIfNull();
        builderContext.AudioReader.ThrowIfNull();

        AudioWriter = builderContext.AudioWriter;
        AudioReader = builderContext.AudioReader;
    }

    public string? InputFilename { get; set; }
    public Stream? InputStream { get; set; }

    public IAudioWriter AudioWriter { get; set; }
    public IAudioReader AudioReader { get; set; }

    public ModelMetadata? ModelMetadata { get; set; }
    public AudioMetadata? AudioMetadata { get; set; }

    public Memory<AudioChunk> InputChunks { get; set; }
    public ConcurrentDictionary<int, AudioChunk[]> OutputChunks { get; set; } = [];
}