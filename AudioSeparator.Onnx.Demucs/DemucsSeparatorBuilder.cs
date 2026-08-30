namespace AudioSeparator.Onnx.Demucs;

using AudioSeparator.Abstractions;
using AudioSeparator.Onnx;

public class DemucsBuilder : OnnxSeparatorBuilder<DemucsBuilder, DemucsSeparatorBuilderContext>
{
    private DemucsBuilder(string modelPath)
    {
        Context.ModelPath = modelPath;
        Context.Requirements.SampleRate = 44100;
        Context.Requirements.StemNames = ["drums", "bass", "other", "vocals"];
    }

    protected override DemucsSeparatorBuilderContext CreateContext()
    {
        return new DemucsSeparatorBuilderContext();
    }

    public static DemucsBuilder Create(string modelPath)
    {
        return new DemucsBuilder(modelPath);
    }

    public override IAudioSeparator Build()
    {
        return new DemucsSeparator(Context);
    }
}
