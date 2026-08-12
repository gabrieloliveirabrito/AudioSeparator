using AudioSeparator.Abstractions;
using AudioSeparator.Abstractions.Audio;
using AudioSeparator.Abstractions.Tasks;

namespace AudioSeparator.Core;

public sealed class SeparationSession : ISeparationSession
{
    private readonly AudioSeparatorContext _context;
    private readonly IReadOnlyList<IProcessTask> _tasks;

    public SeparationSession(
        AudioSeparatorContext context,
        IReadOnlyList<IProcessTask> tasks,
        AudioSourceInfo source)
    {
        _context = context;
        _tasks = tasks;
        Source = source;
    }

    public AudioSourceInfo Source { get; }
    public IReadOnlyList<IProcessTask> Tasks => _tasks;

    public async Task<SeparationResult> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var task in _tasks)
            {
                await task.ExecuteAsync(cancellationToken).WaitAsync(cancellationToken);
            }

            return BuildResult();
        }
        finally
        {
            _context.DisposableResource?.Dispose();
        }
    }

    private SeparationResult BuildResult()
    {
        if (_context.SourceInfo is null)
        {
            throw new InvalidOperationException("Source metadata is missing.");
        }

        var outputSampleRate = _context.Requirements.SampleRate > 0
            ? _context.Requirements.SampleRate
            : _context.SourceInfo.SampleRate;

        var stems = new Dictionary<string, StemAudio>();
        foreach (var (name, chunks) in _context.OutputStems)
        {
            stems[name] = new StemAudio
            {
                Name = name,
                SampleRate = outputSampleRate,
                Channels = _context.InferenceSpec?.OutputChannels ?? _context.SourceInfo.Channels,
                Chunks = chunks
            };
        }

        return new SeparationResult
        {
            Source = _context.SourceInfo,
            Stems = stems,
            Writer = _context.AudioWriter
        };
    }
}
