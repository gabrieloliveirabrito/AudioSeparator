using AudioSeparator.Abstractions.Inference;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx.Mdx.Inference;

public static class MdxInferenceSpecReader
{
    public static InferenceSpec Read(InferenceSession session, MdxModelParams modelParams)
    {
        var inputMeta = session.InputMetadata.First();
        var outputMeta = session.OutputMetadata.First();

        var inputDims = inputMeta.Value.Dimensions.Select(d => d < 0 ? 0 : (int)d).ToArray();
        var outputDims = outputMeta.Value.Dimensions.Select(d => d < 0 ? 0 : (int)d).ToArray();

        // ONNX layout: [batch, 4, dim_f, dim_t]
        if (inputDims.Length >= 4)
        {
            if (inputDims[2] > 0)
            {
                modelParams.DimF = inputDims[2];
            }

            if (inputDims[3] > 0)
            {
                modelParams.DimT = inputDims[3];
            }
        }

        return new InferenceSpec
        {
            InputName = inputMeta.Key,
            InputDimensions = inputDims,
            // Audio channels expected from the reader (not spectrogram channel count).
            InputChannels = 2,
            InputFrameCount = modelParams.ChunkSize,
            OutputName = outputMeta.Key,
            OutputDimensions = outputDims,
            StemCount = 2,
            OutputChannels = 2,
            OutputFrameCount = modelParams.ChunkSize
        };
    }
}
