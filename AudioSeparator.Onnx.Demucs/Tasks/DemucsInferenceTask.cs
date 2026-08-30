using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Core;
using AudioSeparator.Core.Audio;
using AudioSeparator.Core.Tasks;
using AudioSeparator.Onnx;

namespace AudioSeparator.Onnx.Demucs.Tasks;

public class DemucsInferenceTask(OnnxContext context) : ProcessTask("Running inference")
{
    private static readonly SemaphoreSlim RunLock = new(1, 1);

    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        context.InferenceSpec.ThrowIfNull();
        context.SourceInfo.ThrowIfNull();
        context.ProcessingOptions.Validate();

        if (context.InputSamples.Length is 0)
        {
            throw new InvalidOperationException("Input samples are not available in memory.");
        }

        var spec = context.InferenceSpec;
        var stemNames = DemucsChunkInferencer.ResolveStemNames(context, spec);
        var outputStemName = context.ProcessingOptions.OutputStemName;
        if (!string.IsNullOrWhiteSpace(outputStemName) && !stemNames.Contains(outputStemName))
        {
            throw new InvalidOperationException(
                $"Output stem '{outputStemName}' was not found. Available stems: {string.Join(", ", stemNames)}.");
        }

        var channels = spec.OutputChannels;
        var totalFrames = (int)context.SourceInfo.SampleCount;
        var segmentLength = spec.InputFrameCount;
        var enableOverlap = context.ProcessingOptions.EnableOverlapAdd;
        var offsets = AudioWindowPlanner.ComputeOffsets(
            totalFrames,
            segmentLength,
            enableOverlap,
            context.ProcessingOptions.OverlapRatio);

        var stemsToMaterialize = string.IsNullOrWhiteSpace(outputStemName)
            ? stemNames
            : [outputStemName];

        var accumulators = stemsToMaterialize.ToDictionary(
            name => name,
            _ => new OverlapAddAccumulator(totalFrames, channels, segmentLength, enableOverlap));

        var windowBuffer = new float[segmentLength * spec.InputChannels];
        var totalWindows = offsets.Count;
        ReportProgress(0, totalWindows);

        for (var windowIndex = 0; windowIndex < totalWindows; windowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var offset = offsets[windowIndex];
            var frameCount = Math.Min(segmentLength, totalFrames - offset);

            DemucsChunkInferencer.ExtractWindow(
                context.InputSamples,
                spec.InputChannels,
                offset,
                segmentLength,
                windowBuffer);

            await RunLock.WaitAsync(cancellationToken);
            IReadOnlyDictionary<string, ReadOnlyMemory<float>> stemOutputs;
            try
            {
                stemOutputs = DemucsChunkInferencer.InferChunk(
                    context,
                    windowBuffer,
                    frameCount,
                    windowIndex);
            }
            finally
            {
                RunLock.Release();
            }

            foreach (var stemName in stemsToMaterialize)
            {
                var stemSpan = stemOutputs[stemName].Span;
                accumulators[stemName].AddSegment(offset, stemSpan, frameCount);
            }

            ReportProgress(windowIndex + 1, totalWindows);
        }

        context.OutputStemSamples.Clear();
        foreach (var (stemName, accumulator) in accumulators)
        {
            context.OutputStemSamples[stemName] = accumulator.Finalize();
        }
    }
}
