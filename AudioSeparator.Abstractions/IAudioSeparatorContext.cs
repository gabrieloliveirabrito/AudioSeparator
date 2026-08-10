using System.Collections.Concurrent;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Model;

namespace AudioSeparator.Abstractions;

public interface IAudioSeparatorContext
{
    IAudioReader AudioReader { get; set; }
    IAudioWriter AudioWriter { get; set; }

    Memory<AudioChunk> InputChunks { get; set; }
    ConcurrentDictionary<int, AudioChunk[]> OutputChunks { get; set; }
}