using System.Collections.Concurrent;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Inference;

namespace AudioSeparator.Abstractions;

public interface IAudioSeparatorContext
{
    IAudioReader AudioReader { get; set; }
    Memory<AudioChunk> InputChunks { get; set; }
    ConcurrentDictionary<string, AudioChunk[]> OutputStems { get; set; }
    InferenceSpec? InferenceSpec { get; set; }
    AudioSourceInfo? SourceInfo { get; set; }
    SeparationRequirements Requirements { get; set; }
}
