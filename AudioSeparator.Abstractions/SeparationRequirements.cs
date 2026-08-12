namespace AudioSeparator.Abstractions;

public class SeparationRequirements
{
    public int SampleRate { get; set; } = 44100;
    public string[] StemNames { get; set; } = [];
}
