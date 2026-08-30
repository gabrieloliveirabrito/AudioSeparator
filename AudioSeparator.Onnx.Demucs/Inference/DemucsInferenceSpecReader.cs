using AudioSeparator.Abstractions.Inference;
using Microsoft.ML.OnnxRuntime;

namespace AudioSeparator.Onnx.Demucs.Inference;

public static class DemucsInferenceSpecReader
{
    public static InferenceSpec Read(InferenceSession session)
    {
        var inputMeta = session.InputMetadata.First();
        var outputMeta = session.OutputMetadata.First();

        var inputDims = inputMeta.Value.Dimensions.Select(d => d < 0 ? 0 : (int)d).ToArray();
        var outputDims = outputMeta.Value.Dimensions.Select(d => d < 0 ? 0 : (int)d).ToArray();

        return new InferenceSpec
        {
            InputName = inputMeta.Key,
            InputDimensions = inputDims,
            InputChannels = inputDims.Length > 1 ? inputDims[^2] : 0,
            InputFrameCount = inputDims.Length > 0 ? inputDims[^1] : 0,
            OutputName = outputMeta.Key,
            OutputDimensions = outputDims,
            StemCount = outputDims.Length > 1 ? outputDims[1] : 1,
            OutputChannels = outputDims.Length > 2 ? outputDims[2] : 0,
            OutputFrameCount = outputDims.Length > 0 ? outputDims[^1] : 0
        };
    }
}
