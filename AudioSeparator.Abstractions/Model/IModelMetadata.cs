namespace AudioSeparator.Abstractions.Model;

public interface IModelMetadata
{
    public int AudioFrequency { get; set; }

    public int InputSize { get; set; }
    public int InputChannels { get; set; }
    public int[] InputDimensions { get; set; }

    public int OutputSize { get; set; }
    public int OutputChannels { get; set; }
    public int OutputStems { get; set; }
    public int[] OutputDimensions { get; set; }
}