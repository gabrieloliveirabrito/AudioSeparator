using System.Runtime.CompilerServices;
using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Tasks;
using AudioSeparator.Core.Tasks;

namespace AudioSeparator.Core;

public abstract class AudioSeparatorBase<TContext>(AudioSeparatorBuilderContext builderContext) : IAudioSeparator
where TContext : AudioSeparatorContext
{
    private bool disposing = false;

    protected virtual TContext CreateContext()
    {
        return (TContext)new AudioSeparatorContext(builderContext);
    }

    protected abstract IEnumerable<IProcessTask> CreateProcessesTask(TContext context);

    public virtual async IAsyncEnumerable<IProcessTask> Separate(string fileName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = CreateContext();
        context.InputFilename = fileName;

        await foreach (var task in ExecuteTasks(context, cancellationToken))
        {
            yield return task;
        }
    }

    private async IAsyncEnumerable<IProcessTask> ExecuteTasks(TContext context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {   
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
