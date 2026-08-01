namespace AudioSeparator.Abstractions.Tasks;

public interface IProcessTask
{
    string Description { get; set; }

    void ReportProgress(int current, int total);

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}