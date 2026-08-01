using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Abstractions;

public interface IAudioSeparator : IDisposable
{
    IAsyncEnumerable<IProcessTask> Separate(string fileName);
}
