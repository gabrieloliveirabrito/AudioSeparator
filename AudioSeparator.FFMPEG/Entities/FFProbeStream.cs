using System.Text.Json.Serialization;

namespace AudioSeparator.FFMPEG.Entities;

public record FFProbeStream
{
    [JsonPropertyName("index")]
    public int Index { get; set; } = default!;

    [JsonPropertyName("codec_name")]
    public string CodecName { get; set; } = default!;

    [JsonPropertyName("codec_long_name")]
    public string CodecLongName { get; set; } = default!;

    [JsonPropertyName("codec_type")]
    public string CodecType { get; set; } = default!;

    [JsonPropertyName("codec_tag_string")]
    public string CodecTagString { get; set; } = default!;

    [JsonPropertyName("codec_tag")]
    public string CodecTag { get; set; } = default!;

    [JsonPropertyName("sample_fmt")]
    public string SampleFormat { get; set; } = default!;

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; } = default!;

    [JsonPropertyName("channels")]
    public int Channels { get; set; } = default!;

    [JsonPropertyName("bits_per_sample")]
    public int BitsPerSample { get; set; } = default!;

    [JsonPropertyName("initial_padding")]
    public int InitialPadding { get; set; } = default!;

    [JsonPropertyName("r_frame_rate")]
    public string RealFrameRate { get; set; } = default!;

    [JsonPropertyName("avg_frame_rate")]
    public string AvarageFrameRate { get; set; } = default!;

    [JsonPropertyName("time_base")]
    public string? TimeBase { get; set; } = default!;

    [JsonPropertyName("duration_ts")]
    public long? DurationTimeSpan { get; set; }

    [JsonPropertyName("duration")]
    public decimal Duration { get; set; } = default!;

    [JsonPropertyName("bit_rate")]
    public string BitRate { get; set; } = default!;
}