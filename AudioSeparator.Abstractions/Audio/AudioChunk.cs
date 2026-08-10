namespace AudioSeparator.Abstractions.Audio;

public record AudioChunk(ReadOnlyMemory<float> Samples, int Index, int Length)
{
}