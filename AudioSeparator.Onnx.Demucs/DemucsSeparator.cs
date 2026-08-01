namespace AudioSeparator.Demucs;

using AudioSeparator.Onnx;

public class DemucsSeparator(string modelPath, DemucsBuilderContext context) : OnnxSeparator(modelPath, context)
{
    
}