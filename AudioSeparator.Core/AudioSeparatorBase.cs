using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Core;

public abstract class AudioSeparatorBase : IAudioSeparator
{
    private bool disposing = false;
    private string modelPath;

    public AudioSeparatorBase(string modelPath)
    {
        this.modelPath = modelPath;

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException("The model path hasn't' been found!", modelPath);
        }
    }

    public IAsyncEnumerable<IProcessTask> Separate(string fileName)
    {
        throw new NotImplementedException();
    }

    public virtual void Dispose()
    {
        if (!disposing)
        {
            disposing = true;
        }
    }
}
