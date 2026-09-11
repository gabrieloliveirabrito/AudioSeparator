using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Core.Tasks;
using AudioSeparator.Onnx.Mdx.Audio;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AudioSeparator.Onnx.Mdx.Tasks;

public class MdxInferenceTask(MdxContext context) : ProcessTask("Running MDX inference")
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

        var parameters = context.ModelParams;
        var primaryName = parameters.PrimaryStem;
        var secondaryName = parameters.SecondaryStem;
        var stemNames = new[] { primaryName, secondaryName };

        var outputStemName = context.ProcessingOptions.OutputStemName;
        if (!string.IsNullOrWhiteSpace(outputStemName) &&
            !stemNames.Contains(outputStemName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Output stem '{outputStemName}' was not found. Available stems: {string.Join(", ", stemNames)}.");
        }

        var channels = 2;
        var totalFrames = (int)context.SourceInfo.SampleCount;
        var mix = ToChannelMajor(context.InputSamples, channels, totalFrames);

        var primary = await DemixAsync(mix, totalFrames, isMatchMix: false, cancellationToken);

        float[][] secondary;
        var needSecondary = string.IsNullOrWhiteSpace(outputStemName) ||
            outputStemName.Equals(secondaryName, StringComparison.OrdinalIgnoreCase);
        var needPrimary = string.IsNullOrWhiteSpace(outputStemName) ||
            outputStemName.Equals(primaryName, StringComparison.OrdinalIgnoreCase);

        if (needSecondary)
        {
            secondary = new float[2][];
            for (var c = 0; c < 2; c++)
            {
                secondary[c] = new float[totalFrames];
                for (var i = 0; i < totalFrames; i++)
                {
                    secondary[c][i] = mix[c][i] - primary[c][i] * parameters.Compensate;
                }
            }
        }
        else
        {
            secondary = [];
        }

        context.OutputStemSamples.Clear();
        if (needPrimary)
        {
            context.OutputStemSamples[primaryName] = ToInterleaved(primary, totalFrames);
        }

        if (needSecondary)
        {
            context.OutputStemSamples[secondaryName] = ToInterleaved(secondary, totalFrames);
        }
    }

    private async Task<float[][]> DemixAsync(
        float[][] mix,
        int totalFrames,
        bool isMatchMix,
        CancellationToken cancellationToken)
    {
        var parameters = context.ModelParams;
        var stft = new MdxStft(parameters.NFft, parameters.HopLength, parameters.DimF);
        var chunkSize = parameters.ChunkSize;
        var trim = parameters.Trim;
        var overlap = isMatchMix ? 0.02f : context.ProcessingOptions.OverlapRatio;
        if (!context.ProcessingOptions.EnableOverlapAdd && !isMatchMix)
        {
            overlap = 0f;
        }

        var genSize = chunkSize - 2 * trim;
        if (genSize <= 0)
        {
            throw new InvalidOperationException(
                $"Invalid MDX chunk geometry: chunk={chunkSize}, trim={trim}.");
        }

        var pad = genSize + trim - (totalFrames % genSize);
        if (pad == genSize + trim)
        {
            pad = trim;
        }

        var mixtureLength = trim + totalFrames + pad;
        var mixture = new float[2][];
        for (var c = 0; c < 2; c++)
        {
            mixture[c] = new float[mixtureLength];
            Array.Copy(mix[c], 0, mixture[c], trim, totalFrames);
        }

        var step = Math.Max(1, (int)((1f - overlap) * chunkSize));
        var result = new float[2][];
        var divider = new float[2][];
        for (var c = 0; c < 2; c++)
        {
            result[c] = new float[mixtureLength];
            divider[c] = new float[mixtureLength];
        }

        var totalChunks = (mixtureLength + step - 1) / step;
        ReportProgress(0, totalChunks);
        var chunkIndex = 0;

        for (var start = 0; start < mixtureLength; start += step)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var end = Math.Min(start + chunkSize, mixtureLength);
            var actual = end - start;

            var chunk = new float[2][];
            for (var c = 0; c < 2; c++)
            {
                chunk[c] = new float[chunkSize];
                Array.Copy(mixture[c], start, chunk[c], 0, actual);
            }

            float[][] waves;
            if (isMatchMix)
            {
                waves = chunk;
            }
            else
            {
                await RunLock.WaitAsync(cancellationToken);
                try
                {
                    waves = RunModel(chunk, stft, parameters);
                }
                finally
                {
                    RunLock.Release();
                }
            }

            if (overlap > 0f)
            {
                var window = CreateHanning(actual);
                for (var i = 0; i < actual; i++)
                {
                    var w = window[i];
                    for (var c = 0; c < 2; c++)
                    {
                        result[c][start + i] += waves[c][i] * w;
                        divider[c][start + i] += w;
                    }
                }
            }
            else
            {
                for (var i = 0; i < actual; i++)
                {
                    for (var c = 0; c < 2; c++)
                    {
                        result[c][start + i] += waves[c][i];
                        divider[c][start + i] += 1f;
                    }
                }
            }

            chunkIndex++;
            ReportProgress(chunkIndex, totalChunks);
        }

        var primary = new float[2][];
        for (var c = 0; c < 2; c++)
        {
            primary[c] = new float[totalFrames];
            for (var i = 0; i < totalFrames; i++)
            {
                var index = i + trim;
                var div = divider[c][index];
                primary[c][i] = div > 1e-8f ? result[c][index] / div : result[c][index];
            }
        }

        return primary;
    }

    private float[][] RunModel(float[][] chunk, MdxStft stft, MdxModelParams parameters)
    {
        var spec = context.InferenceSpec!;
        var dimT = parameters.DimT;
        var spek = stft.Forward(chunk, dimT);
        stft.ZeroLowBins(spek, dimT);

        float[] prediction;
        if (parameters.EnableDenoise)
        {
            var neg = (float[])spek.Clone();
            for (var i = 0; i < neg.Length; i++)
            {
                neg[i] = -neg[i];
            }

            var negOut = RunOnnx(neg, spec.InputName, parameters);
            var posOut = RunOnnx(spek, spec.InputName, parameters);
            prediction = new float[posOut.Length];
            for (var i = 0; i < prediction.Length; i++)
            {
                prediction[i] = (posOut[i] * 0.5f) + (negOut[i] * -0.5f);
            }
        }
        else
        {
            prediction = RunOnnx(spek, spec.InputName, parameters);
        }

        return stft.Inverse(prediction, dimT, parameters.ChunkSize);
    }

    private float[] RunOnnx(float[] spek, string inputName, MdxModelParams parameters)
    {
        var tensor = new DenseTensor<float>(spek, [1, 4, parameters.DimF, parameters.DimT]);
        using var outputs = context.Session.Run([
            NamedOnnxValue.CreateFromTensor(inputName, tensor)
        ]);

        var output = outputs.First().AsTensor<float>();
        var buffer = new float[4 * parameters.DimF * parameters.DimT];
        for (var channel = 0; channel < 4; channel++)
        {
            for (var freq = 0; freq < parameters.DimF; freq++)
            {
                for (var frame = 0; frame < parameters.DimT; frame++)
                {
                    buffer[(channel * parameters.DimF + freq) * parameters.DimT + frame] =
                        output[0, channel, freq, frame];
                }
            }
        }

        return buffer;
    }

    private static float[][] ToChannelMajor(float[] interleaved, int channels, int frames)
    {
        var result = new float[channels][];
        for (var c = 0; c < channels; c++)
        {
            result[c] = new float[frames];
            for (var i = 0; i < frames; i++)
            {
                result[c][i] = interleaved[i * channels + c];
            }
        }

        return result;
    }

    private static float[] ToInterleaved(float[][] channelMajor, int frames)
    {
        var channels = channelMajor.Length;
        var interleaved = new float[frames * channels];
        for (var i = 0; i < frames; i++)
        {
            for (var c = 0; c < channels; c++)
            {
                interleaved[i * channels + c] = channelMajor[c][i];
            }
        }

        return interleaved;
    }

    private static float[] CreateHanning(int length)
    {
        var window = new float[length];
        if (length == 1)
        {
            window[0] = 1f;
            return window;
        }

        for (var i = 0; i < length; i++)
        {
            window[i] = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * i / (length - 1));
        }

        return window;
    }
}
