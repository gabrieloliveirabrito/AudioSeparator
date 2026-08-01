namespace AudioSeparator.Core.Entities;

public sealed class ModelMetadata
{
    public required int Batches { get; set; }
    public required string[] InputNames { get; set; }
    public required string[] OutputNames { get; set; }

    public required int InputChannels { get; set; }
    public required int OutputChannels { get; set; }

    public required int InputBufferSize { get; set; }
    public required int OutputBufferSize { get; set; }
}