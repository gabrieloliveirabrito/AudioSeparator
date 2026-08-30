namespace AudioSeparator.Abstractions;

public static class StemAudioStream
{
    public static void ResetPosition(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
    }

    public static async Task CopyPcmToAsync(
        Stream source,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ResetPosition(source);

        var buffer = new byte[8192];
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read is 0)
            {
                break;
            }

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
    }

    public static byte[] ReadPcmBytes(Stream source)
    {
        ResetPosition(source);

        if (source is MemoryStream memoryStream)
        {
            var start = (int)memoryStream.Position;
            var length = (int)(memoryStream.Length - memoryStream.Position);
            var bytes = new byte[length];
            memoryStream.ReadExactly(bytes);
            return bytes;
        }

        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        return buffer.ToArray();
    }
}
