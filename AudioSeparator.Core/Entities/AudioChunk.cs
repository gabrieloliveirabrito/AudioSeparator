namespace AudioSeparator.Core.Entities;

public record AudioChunk(int Index, float[] LeftSamples, float[] RightSamples)
{
    public int Length { get; set; } = LeftSamples.Length;
}