namespace AudioSeparator.Onnx.Demucs;

using AudioSeparator.Onnx;

public class DemucsBuilder : OnnxSeparatorBuilder<DemucsBuilder, DemucsSeparatorBuilderContext>
{
    protected DemucsBuilder(string modelPath) : base()
    {
        
    }

    protected override DemucsSeparatorBuilderContext CreateContext()
    {
        return new DemucsSeparatorBuilderContext();
    }

    public static DemucsBuilder Create(string modelPath)
    {
        return new(modelPath);
    }

    public override DemucsSeparator Build()
    {
        return new DemucsSeparator(Context);
    }
}