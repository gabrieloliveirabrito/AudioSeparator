using System.Text.Json.Serialization;

namespace AudioSeparator.FFMPEG.Entities;

public record FFProbeFormat
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = default!;

    [JsonPropertyName("nb_streams")]
    public int StreamCount { get; set; } = default!;

    [JsonPropertyName("nb_programs")]
    public int ProgramCount { get; set; } = default!;

    [JsonPropertyName("nb_stream_groups")]
    public int StreamGroups { get; set; } = default!;

    [JsonPropertyName("format_name")]
    public string FormatName { get; set; } = default!;

    [JsonPropertyName("format_long_name")]
    public string FormatLongName { get; set; } = default!;

    [JsonPropertyName("duration")]
    public decimal Duration { get; set; } = default!;

    [JsonPropertyName("size")]
    public long Size { get; set; } = default!;

    [JsonPropertyName("bit_rate")]
    public long BitRate { get; set; } = default!;

    [JsonPropertyName("probe_score")]
    public int ProbeScore { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; } = [];
}