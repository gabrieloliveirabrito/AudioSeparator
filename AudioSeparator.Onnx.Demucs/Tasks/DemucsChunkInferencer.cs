using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Core;
using AudioSeparator.Onnx;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AudioSeparator.Onnx.Demucs.Tasks;

public static class DemucsChunkInferencer
{
    public static IReadOnlyDictionary<string, ReadOnlyMemory<float>> InferChunk(
        OnnxContext context,
        ReadOnlySpan<float> windowSamples,
        int frameCount,
        int _)
    {
        context.InferenceSpec.ThrowIfNull();

        var spec = context.InferenceSpec;
        var stemNames = ResolveStemNames(context, spec);
        var tensor = new DenseTensor<float>([1, spec.InputChannels, spec.InputFrameCount]);

        for (var frame = 0; frame < spec.InputFrameCount; frame++)
        {
            if (frame >= frameCount)
            {
                continue;
            }

            var sampleIndex = frame * spec.InputChannels;
            tensor[0, 0, frame] = windowSamples[sampleIndex];
            if (spec.InputChannels > 1)
            {
                tensor[0, 1, frame] = windowSamples[sampleIndex + 1];
            }
        }

        using var outputs = context.Session.Run([
            NamedOnnxValue.CreateFromTensor(spec.InputName, tensor)
        ]);

        var output = outputs.First().AsTensor<float>();
        var dataSize = Math.Min(frameCount, spec.OutputFrameCount);
        var result = new Dictionary<string, ReadOnlyMemory<float>>(stemNames.Length);

        for (var stemIndex = 0; stemIndex < spec.StemCount; stemIndex++)
        {
            var stemData = new float[dataSize * spec.OutputChannels];
            for (var frame = 0; frame < dataSize; frame++)
            {
                for (var channel = 0; channel < spec.OutputChannels; channel++)
                {
                    stemData[frame * spec.OutputChannels + channel] =
                        output[0, stemIndex, channel, frame];
                }
            }

            result[stemNames[stemIndex]] = stemData;
        }

        return result;
    }

    public static string[] ResolveStemNames(AudioSeparatorContext context, InferenceSpec spec)
    {
        if (context.Requirements.StemNames.Length == spec.StemCount)
        {
            return context.Requirements.StemNames;
        }

        if (context.Requirements.StemNames.Length > 0)
        {
            throw new InvalidOperationException(
                $"Expected {spec.StemCount} stem names, but got {context.Requirements.StemNames.Length}.");
        }

        return Enumerable.Range(0, spec.StemCount).Select(i => $"stem_{i}").ToArray();
    }

    public static void ExtractWindow(
        ReadOnlySpan<float> inputSamples,
        int channels,
        int offsetFrames,
        int segmentLength,
        Span<float> destination)
    {
        destination.Clear();

        var totalFrames = inputSamples.Length / channels;
        var frameCount = Math.Min(segmentLength, totalFrames - offsetFrames);
        if (frameCount <= 0)
        {
            return;
        }

        var sourceStart = offsetFrames * channels;
        var copyLength = frameCount * channels;
        inputSamples.Slice(sourceStart, copyLength).CopyTo(destination[..copyLength]);
    }
}
