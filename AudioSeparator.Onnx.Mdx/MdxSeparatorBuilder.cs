using AudioSeparator.Abstractions;
using AudioSeparator.Onnx;

namespace AudioSeparator.Onnx.Mdx;

public class MdxBuilder : OnnxSeparatorBuilder<MdxBuilder, MdxSeparatorBuilderContext>
{
    private MdxBuilder(string modelPath)
    {
        Context.ModelPath = modelPath;
        Context.Requirements.SampleRate = 44100;
        Context.Requirements.StemNames = ["instrumental", "vocals"];
        Context.ProcessingOptions.EnableOverlapAdd = true;
        Context.ProcessingOptions.OverlapRatio = 0.25f;
        Context.ProcessingOptions.EnablePeakNormalize = true;
        Context.ProcessingOptions.ClipPreventMode = ClipPreventMode.Rescale;
        Context.ProcessingOptions.EnableResample = true;
        Context.ModelParams = MdxModelParams.CreateInstHq5Defaults();
    }

    protected override MdxSeparatorBuilderContext CreateContext() => new();

    public static MdxBuilder Create(string modelPath) => new(modelPath);

    public MdxBuilder WithModelParams(MdxModelParams parameters)
    {
        Context.ModelParams = parameters;
        Context.Requirements.StemNames = [parameters.PrimaryStem, parameters.SecondaryStem];
        return this;
    }

    public MdxBuilder WithDenoise(bool enabled = true)
    {
        Context.ModelParams.EnableDenoise = enabled;
        return this;
    }

    public MdxBuilder WithCompensate(float compensate)
    {
        Context.ModelParams.Compensate = compensate;
        return this;
    }

    public override IAudioSeparator Build() => new MdxSeparator(Context);
}
