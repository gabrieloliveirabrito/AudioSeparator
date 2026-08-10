using System.Text.Json.Serialization;

namespace AudioSeparator.FFMPEG.Entities;

public record FFProbeResult
{
    [JsonPropertyName("streams")]
    public List<FFProbeStream> Streams { get; set; } = default!;

    [JsonPropertyName("format")]
    public FFProbeFormat Format { get; set; } = default!;
}