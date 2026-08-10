using System.Runtime.CompilerServices;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Core;

public abstract class AudioSeparatorBase<TContext> : IAudioSeparator
where TContext : AudioSeparatorContext
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

    protected abstract TContext CreateContext();
    protected abstract IEnumerable<IProcessTask> CreateProcessesTask(TContext context);

    public virtual async IAsyncEnumerable<IProcessTask> Separate(string fileName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = CreateContext();
        context.InputFilename = fileName;
        context.ModelFilename = modelPath;
        
        var tasks = new List<Func<CancellationToken, Task>>();

        foreach (var processTask in CreateProcessesTask(context))
        {
            yield return processTask;
            tasks.Add(processTask.ExecuteAsync);
        }

        foreach (var task in tasks)
        {
            await task(cancellationToken).WaitAsync(cancellationToken);
        }
    }

    public virtual void Dispose()
    {
        if (!disposing)
        {
            disposing = true;
        }
    }
}
