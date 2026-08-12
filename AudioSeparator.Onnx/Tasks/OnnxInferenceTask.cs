using System.Runtime.InteropServices;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Inference;
using AudioSeparator.Core;
using AudioSeparator.Core.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AudioSeparator.Onnx.Tasks;

public class OnnxInferenceTask(OnnxContext context) : ProcessTask("Running inference")
{
    private static readonly SemaphoreSlim RunLock = new(1, 1);

    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        context.InferenceSpec.ThrowIfNull();

        var inputChunks = context.InputChunks.ToArray();
        if (inputChunks.Length is 0)
        {
            throw new InvalidOperationException("Input chunks are not available in memory.");
        }

        var spec = context.InferenceSpec;
        var stemNames = ResolveStemNames(context, spec);
        var stemBuffers = stemNames.ToDictionary(
            name => name,
            _ => new AudioChunk[inputChunks.Length]);

        var totalChunks = inputChunks.Length;
        ReportProgress(0, totalChunks);

        for (var chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunk = inputChunks[chunkIndex];

            await RunLock.WaitAsync(cancellationToken);
            try
            {
                var tensor = new DenseTensor<float>([1, spec.InputChannels, spec.InputFrameCount]);
                for (var frame = 0; frame < chunk.Length; frame++)
                {
                    tensor[0, 0, frame] = chunk.Samples.Span[frame * spec.InputChannels];
                    if (spec.InputChannels > 1)
                    {
                        tensor[0, 1, frame] = chunk.Samples.Span[frame * spec.InputChannels + 1];
                    }
                }

                using var outputs = context.Session.Run([
                    NamedOnnxValue.CreateFromTensor(spec.InputName, tensor)
                ]);

                var output = outputs.First().AsTensor<float>();
                var dataSize = Math.Min(chunk.Length, spec.OutputFrameCount);

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

                    stemBuffers[stemNames[stemIndex]][chunkIndex] =
                        new AudioChunk(stemData, chunk.Index, dataSize);
                }
            }
            finally
            {
                RunLock.Release();
            }

            ReportProgress(chunkIndex + 1, totalChunks);
        }

        foreach (var (name, chunks) in stemBuffers)
        {
            context.OutputStems[name] = chunks;
        }
    }

    private static string[] ResolveStemNames(AudioSeparatorContext context, InferenceSpec spec)
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
}
