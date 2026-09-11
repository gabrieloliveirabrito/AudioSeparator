namespace AudioSeparator.Core.Audio;

public static class PeakNormalizer
{
    public static float FindPeak(ReadOnlySpan<float> samples)
    {
        var peak = 0f;
        foreach (var sample in samples)
        {
            var abs = MathF.Abs(sample);
            if (abs > peak)
            {
                peak = abs;
            }
        }

        return peak;
    }

    public static void ScaleInPlace(Span<float> samples, float scale)
    {
        if (scale is 0f or 1f)
        {
            return;
        }

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] *= scale;
        }
    }
}
