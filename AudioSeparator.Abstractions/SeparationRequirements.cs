namespace AudioSeparator.Abstractions;

public class SeparationRequirements
{
    /// <summary>
    /// Expected input sample rate in Hz. When zero, sample rate is not validated
    /// and output stems inherit the source file sample rate.
    /// </summary>
    public int SampleRate { get; set; }

    public string[] StemNames { get; set; } = [];
}
