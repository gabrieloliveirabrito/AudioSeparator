namespace AudioSeparator.Abstractions.Audio;

public class AudioMetadata
{
    public int SampleRate { get; set; }
    public long SampleCount { get; set; }
    
    public int Channels { get; set; }
    public int ChunkCount { get; set; }
}