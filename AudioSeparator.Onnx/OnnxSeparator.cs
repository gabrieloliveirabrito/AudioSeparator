using AudioSeparator.Core;

namespace AudioSeparator.Onnx;

public abstract class OnnxSeparator(string modelPath, OnnxBuilderContext context) : AudioSeparatorBase(modelPath)
{
    
}