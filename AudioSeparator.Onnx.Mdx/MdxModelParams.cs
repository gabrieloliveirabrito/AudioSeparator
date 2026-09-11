namespace AudioSeparator.Onnx.Mdx;

/// <summary>
/// Host-side MDX-Net parameters (STFT + stem metadata). ONNX only sees spectrogram tensors.
/// </summary>
public sealed class MdxModelParams
{
    public int NFft { get; set; } = 7680;

    public int HopLength { get; set; } = 1024;

    public int DimF { get; set; } = 3072;

    public int DimT { get; set; } = 256;

    public float Compensate { get; set; } = 1.021f;

    public string PrimaryStem { get; set; } = "instrumental";

    public string SecondaryStem { get; set; } = "vocals";

    public bool EnableDenoise { get; set; }

    public int ChunkSize => HopLength * (DimT - 1);

    public int Trim => NFft / 2;

    public int FrequencyBins => NFft / 2 + 1;

    public static MdxModelParams CreateInstHq5Defaults() => new();
}
