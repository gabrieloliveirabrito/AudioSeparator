using AudioSeparator.Abstractions.Audio;

namespace AudioSeparator.NAudio;

public record NAudioChunk(float[][] Samples, int Index, int Length) : IAudioChunk
{
}