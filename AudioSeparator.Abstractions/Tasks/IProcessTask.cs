namespace AudioSeparator.Abstractions.Tasks;

public interface IProcessTask
{
    string Description { get; set; }

    void SetProgressCallback(Action<long, long> callback);
    void ClearProgressCallback();
    void ReportProgress(long current, long total);

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}