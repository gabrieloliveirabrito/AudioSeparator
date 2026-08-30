using System.Runtime.InteropServices;
using AudioSeparator.Abstractions;

namespace AudioSeparator.Core.Audio;

public static class StemAudioBuffer
{
    public static MemoryStream CreatePcmStream(ReadOnlySpan<float> samples)
    {
        var byteCount = samples.Length * sizeof(float);
        var stream = new MemoryStream(byteCount);
        var bytes = MemoryMarshal.AsBytes(samples);
        stream.Write(bytes);
        stream.Position = 0;
        return stream;
    }

    public static void ResetStreamPosition(Stream stream) => StemAudioStream.ResetPosition(stream);

    public static async Task CopyPcmToAsync(
        Stream source,
        Stream destination,
        CancellationToken cancellationToken = default) =>
        await StemAudioStream.CopyPcmToAsync(source, destination, cancellationToken);
}
