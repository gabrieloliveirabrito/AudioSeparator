using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace AudioSeparator.Onnx.Mdx.Audio;

/// <summary>
/// Host STFT/iSTFT matching UVR / audio-separator MDX (Hann, center=true, dim_f crop).
/// </summary>
public sealed class MdxStft
{
    private readonly int _nFft;
    private readonly int _hopLength;
    private readonly int _dimF;
    private readonly int _nBins;
    private readonly float[] _window;

    public MdxStft(int nFft, int hopLength, int dimF)
    {
        _nFft = nFft;
        _hopLength = hopLength;
        _dimF = dimF;
        _nBins = nFft / 2 + 1;
        _window = CreateHannWindow(nFft);
    }

    /// <summary>
    /// Forward STFT. <paramref name="channelMajor"/> is [channels, samples] with channels=2.
    /// Returns spectrogram as [4, dim_f, dim_t] laid out as realL, imagL, realR, imagR.
    /// </summary>
    public float[] Forward(float[][] channelMajor, int dimT)
    {
        if (channelMajor.Length != 2)
        {
            throw new ArgumentException("MDX STFT expects stereo input.", nameof(channelMajor));
        }

        var pad = _nFft / 2;
        var spectrogram = new float[4 * _dimF * dimT];

        for (var channel = 0; channel < 2; channel++)
        {
            var padded = PadCenter(channelMajor[channel], pad);
            for (var frame = 0; frame < dimT; frame++)
            {
                var start = frame * _hopLength;
                var spectrum = new Complex32[_nFft];
                for (var i = 0; i < _nFft; i++)
                {
                    spectrum[i] = new Complex32(padded[start + i] * _window[i], 0f);
                }

                Fourier.Forward(spectrum, FourierOptions.Matlab);

                var baseReal = channel * 2;
                var baseImag = channel * 2 + 1;
                for (var freq = 0; freq < _dimF; freq++)
                {
                    var index = ((baseReal * _dimF) + freq) * dimT + frame;
                    var indexImag = ((baseImag * _dimF) + freq) * dimT + frame;
                    spectrogram[index] = spectrum[freq].Real;
                    spectrogram[indexImag] = spectrum[freq].Imaginary;
                }
            }
        }

        return spectrogram;
    }

    /// <summary>
    /// Inverse STFT. <paramref name="spectrogram"/> is [4, dim_f, dim_t] same layout as <see cref="Forward"/>.
    /// Returns [channels][samples] of length <paramref name="outputSamples"/>.
    /// </summary>
    public float[][] Inverse(ReadOnlySpan<float> spectrogram, int dimT, int outputSamples)
    {
        var pad = _nFft / 2;
        var channels = new float[2][];
        var windowSum = new float[outputSamples + 2 * pad + _nFft];

        for (var channel = 0; channel < 2; channel++)
        {
            var accumulator = new float[outputSamples + 2 * pad + _nFft];
            var baseReal = channel * 2;
            var baseImag = channel * 2 + 1;

            for (var frame = 0; frame < dimT; frame++)
            {
                var spectrum = new Complex32[_nFft];
                for (var freq = 0; freq < _dimF; freq++)
                {
                    var real = spectrogram[((baseReal * _dimF) + freq) * dimT + frame];
                    var imag = spectrogram[((baseImag * _dimF) + freq) * dimT + frame];
                    spectrum[freq] = new Complex32(real, imag);
                }

                // Mirror conjugate for negative frequencies (real signal).
                for (var freq = 1; freq < _nBins - 1 && _nFft - freq < _nFft; freq++)
                {
                    spectrum[_nFft - freq] = Complex32.Conjugate(spectrum[freq]);
                }

                Fourier.Inverse(spectrum, FourierOptions.Matlab);

                var start = frame * _hopLength;
                for (var i = 0; i < _nFft; i++)
                {
                    var weight = _window[i];
                    accumulator[start + i] += spectrum[i].Real * weight;
                    if (channel == 0)
                    {
                        windowSum[start + i] += weight * weight;
                    }
                }
            }

            channels[channel] = new float[outputSamples];
            for (var i = 0; i < outputSamples; i++)
            {
                var weight = windowSum[i + pad];
                channels[channel][i] = weight > 1e-8f
                    ? accumulator[i + pad] / weight
                    : accumulator[i + pad];
            }
        }

        return channels;
    }

    public void ZeroLowBins(Span<float> spectrogram, int dimT, int bins = 3)
    {
        for (var channel = 0; channel < 4; channel++)
        {
            for (var freq = 0; freq < bins && freq < _dimF; freq++)
            {
                for (var frame = 0; frame < dimT; frame++)
                {
                    spectrogram[(channel * _dimF + freq) * dimT + frame] = 0f;
                }
            }
        }
    }

    private static float[] PadCenter(ReadOnlySpan<float> samples, int pad)
    {
        var padded = new float[samples.Length + 2 * pad];
        samples.CopyTo(padded.AsSpan(pad));
        return padded;
    }

    private static float[] CreateHannWindow(int length)
    {
        var window = new float[length];
        for (var i = 0; i < length; i++)
        {
            window[i] = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * i / length);
        }

        return window;
    }
}
