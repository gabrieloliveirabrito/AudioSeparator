using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Core.Tasks;

public abstract class ProcessTask(string description) : IProcessTask
{
    private Action<long, long>? progressCallback;

    public string Description { get; set; } = description;

    public void SetProgressCallback(Action<long, long> callback)
    {
        progressCallback = callback;
    }

    public void ClearProgressCallback()
    {
        progressCallback = null;
    }

    public void ReportProgress(long current, long total)
    {
        progressCallback?.Invoke(current, total);
    }

    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}