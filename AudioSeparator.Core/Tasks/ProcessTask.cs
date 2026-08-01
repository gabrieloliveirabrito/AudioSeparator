namespace AudioSeparator.Core.Tasks;

public abstract class ProcessTask(string Description)
{
    private Action<long, long>? progressCallback;

    public void SetProgressCallback(Action<long, long> callback)
    {
        progressCallback = callback;
    }

    public void ClearProgressCallback()
    {
        progressCallback = null;
    }

    protected void ReportProgress(long current, long total)
    {
        progressCallback?.Invoke(current, total);
    }

    public abstract Task ExecuteAsync(AudioSeparatorContext context, CancellationToken cancellationToken = default);
}