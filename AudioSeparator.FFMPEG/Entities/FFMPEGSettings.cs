namespace AudioSeparator.FFMPEG.Entities;

public class FFMPEGSettings
{
    public string OutputCodec { get; set; } = "pcm_s16le";
    public string OutputFormat { get; set; } = "wav";
}