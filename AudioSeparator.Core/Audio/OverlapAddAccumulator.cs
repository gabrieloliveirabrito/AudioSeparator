namespace AudioSeparator.Core.Audio;

public sealed class OverlapAddAccumulator
{
    private readonly float[] _sum;
    private readonly float[]? _weightSum;
    private readonly float[]? _window;
    private readonly int _channels;
    private readonly int _totalFrames;
    private readonly bool _enableOverlap;

    public OverlapAddAccumulator(int totalFrames, int channels, int segmentLength, bool enableOverlap)
    {
        if (totalFrames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalFrames));
        }

        if (channels <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channels));
        }

        if (segmentLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentLength));
        }

        _totalFrames = totalFrames;
        _channels = channels;
        _enableOverlap = enableOverlap;
        _sum = new float[totalFrames * channels];

        if (enableOverlap)
        {
            _weightSum = new float[totalFrames];
            _window = CreateTriangularWindow(segmentLength);
        }
    }

    public void AddSegment(int offsetFrames, ReadOnlySpan<float> interleavedSamples, int frameCount)
    {
        if (frameCount <= 0)
        {
            return;
        }

        if (!_enableOverlap)
        {
            var destination = _sum.AsSpan(offsetFrames * _channels, frameCount * _channels);
            interleavedSamples[..(frameCount * _channels)].CopyTo(destination);
            return;
        }

        var window = _window!;
        var weightSum = _weightSum!;

        for (var frame = 0; frame < frameCount; frame++)
        {
            var outputFrame = offsetFrames + frame;
            if (outputFrame >= _totalFrames)
            {
                break;
            }

            var weight = window[frame];
            weightSum[outputFrame] += weight;

            var sourceIndex = frame * _channels;
            var destinationIndex = outputFrame * _channels;
            for (var channel = 0; channel < _channels; channel++)
            {
                _sum[destinationIndex + channel] += interleavedSamples[sourceIndex + channel] * weight;
            }
        }
    }

    public float[] Finalize()
    {
        if (!_enableOverlap)
        {
            return _sum;
        }

        var weightSum = _weightSum!;
        for (var frame = 0; frame < _totalFrames; frame++)
        {
            var weight = weightSum[frame];
            if (weight <= 0f)
            {
                continue;
            }

            var frameIndex = frame * _channels;
            for (var channel = 0; channel < _channels; channel++)
            {
                _sum[frameIndex + channel] /= weight;
            }
        }

        return _sum;
    }

    private static float[] CreateTriangularWindow(int segmentLength)
    {
        var half = segmentLength / 2;
        var window = new float[segmentLength];

        for (var i = 0; i < half; i++)
        {
            window[i] = i + 1;
        }

        for (var i = half; i < segmentLength; i++)
        {
            window[i] = segmentLength - i;
        }

        var max = (float)window.Max();
        for (var i = 0; i < segmentLength; i++)
        {
            window[i] /= max;
        }

        return window;
    }
}
