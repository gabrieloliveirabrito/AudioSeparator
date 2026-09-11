namespace AudioSeparator.Core.Audio;

using AudioSeparator.Abstractions;

public static class ClipPrevention
{
    public static void ApplyInPlace(Span<float> samples, ClipPreventMode mode)
    {
        switch (mode)
        {
            case ClipPreventMode.None:
                return;
            case ClipPreventMode.Rescale:
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

                var scale = MathF.Max(1.01f * peak, 1f);
                if (scale <= 1f)
                {
                    return;
                }

                for (var i = 0; i < samples.Length; i++)
                {
                    samples[i] /= scale;
                }

                return;
            }
            case ClipPreventMode.Clamp:
                for (var i = 0; i < samples.Length; i++)
                {
                    samples[i] = Math.Clamp(samples[i], -0.99f, 0.99f);
                }

                return;
            case ClipPreventMode.Tanh:
                for (var i = 0; i < samples.Length; i++)
                {
                    samples[i] = MathF.Tanh(samples[i]);
                }

                return;
            default:
                throw new InvalidOperationException($"Invalid clip prevent mode: {mode}.");
        }
    }
}
