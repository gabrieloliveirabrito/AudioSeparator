namespace AudioSeparator.Abstractions.Inference;

public sealed class InferenceSpec
{
    public string InputName { get; init; } = "input";
    public int InputFrameCount { get; init; }
    public int InputChannels { get; init; }
    public int[] InputDimensions { get; init; } = [];

    public string OutputName { get; init; } = "output";
    public int OutputFrameCount { get; init; }
    public int OutputChannels { get; init; }
    public int StemCount { get; init; }
    public int[] OutputDimensions { get; init; } = [];
}
