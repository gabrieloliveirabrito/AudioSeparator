namespace AudioSeparator.Abstractions.Audio;

public interface IAudioChunk
{
    float[][] Samples { get; }
    int Index { get; }
    int Length { get; }
}