using System.Collections.Concurrent;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Core;

public class AudioSeparatorContext : IAudioSeparatorContext
{    
    public string? ModelFilename { get; set; }
    public string? InputFilename { get; set; }
    public Stream? InputStream { get; set; }

    public required IAudioWriter AudioWriter { get; set; }
    public required IAudioReader AudioReader { get; set; }

    public ModelMetadata? ModelMetadata { get; set; }
    public AudioMetadata? AudioMetadata { get; set; }

    public Memory<AudioChunk> InputChunks { get; set; }
    public ConcurrentDictionary<int, AudioChunk[]> OutputChunks { get; set; } = [];
}