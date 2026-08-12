namespace AudioSeparator.Onnx.Demucs;

using AudioSeparator.Onnx;

public class DemucsSeparator(DemucsSeparatorBuilderContext context) : OnnxSeparator<DemucsContext>(context)
{
}
