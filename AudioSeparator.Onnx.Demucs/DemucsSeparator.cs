namespace AudioSeparator.Onnx.Demucs;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Extensions;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Core.Tasks;
using AudioSeparator.Onnx;

public class DemucsAudioReadTask(DemucsContext context) : AudioReadTask(context)
{
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await base.ExecuteAsync(cancellationToken);
        Console.WriteLine(context.InputChunks.Length);

        if(MemoryMarshal.TryGetArray<AudioChunk>(context.InputChunks, out var segment))
        {
            context.OutputChunks[0] = segment.Array ?? throw new NullReferenceException("Invalid audio memory segment");
            return;
        }

        throw new InvalidOperationException("Failed to retrieve the memory segment");
    }
}

public class DemucsSeparator(string modelPath, DemucsBuilderContext context) : OnnxSeparator<DemucsContext>(modelPath, context)
{
    protected override DemucsContext CreateContext()
    {
        context.AudioReader.ThrowIfNull();
        context.AudioWriter.ThrowIfNull();

        return new DemucsContext
        {
            AudioReader = context.AudioReader,
            AudioWriter = context.AudioWriter,
            ModelMetadata = new Abstractions.Model.ModelMetadata
            {
                AudioFrequency = 44100,
                InputChannels = 2,
                OutputChannels = 2,
                InputSize = 343980,
                OutputSize = 343980,
                OutputStems = 1
            }
        };
    }

    protected override IEnumerable<IProcessTask> CreateProcessesTask(DemucsContext context)
    {
        //yield return new AudioMetadataTask(context);
        yield return new DemucsAudioReadTask(context);
        yield return new AudioWriteTask(context);
    }
}