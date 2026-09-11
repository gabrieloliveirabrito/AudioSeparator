namespace AudioSeparator.Core.Audio;

/// <summary>
/// Linear-interpolation resampler for interleaved PCM. Hobby-quality; not a pro-audio SRC.
/// </summary>
public static class LinearResampler
{
    public static float[] Resample(ReadOnlySpan<float> input, int channels, int sourceRate, int targetRate)
    {
        if (channels <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channels));
        }

        if (sourceRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceRate));
        }

        if (targetRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetRate));
        }

        if (sourceRate == targetRate)
        {
            return input.ToArray();
        }

        var inputFrames = input.Length / channels;
        if (inputFrames <= 0)
        {
            return [];
        }

        var outputFrames = (int)((long)inputFrames * targetRate / sourceRate);
        if (outputFrames <= 0)
        {
            return [];
        }

        var output = new float[outputFrames * channels];
        var ratio = (double)sourceRate / targetRate;

        for (var outFrame = 0; outFrame < outputFrames; outFrame++)
        {
            var srcPos = outFrame * ratio;
            var srcIndex = (int)srcPos;
            var frac = (float)(srcPos - srcIndex);
            var nextIndex = Math.Min(srcIndex + 1, inputFrames - 1);

            for (var channel = 0; channel < channels; channel++)
            {
                var a = input[srcIndex * channels + channel];
                var b = input[nextIndex * channels + channel];
                output[outFrame * channels + channel] = a + (b - a) * frac;
            }
        }

        return output;
    }
}
