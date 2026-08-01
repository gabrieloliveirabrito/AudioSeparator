namespace AudioSeparator.Demucs;

using AudioSeparator.Onnx;

public class DemucsBuilder : OnnxSeparatorBuilder<DemucsBuilder, DemucsBuilderContext>
{
    protected DemucsBuilder() : base()
    {
        
    }

    protected override DemucsBuilderContext CreateContext()
    {
        return new DemucsBuilderContext();
    }

    public static DemucsBuilder Create()
    {
        return new();
    }

    public override DemucsSeparator Build(string modelPath)
    {
        return new DemucsSeparator(modelPath, Context);
    }
}