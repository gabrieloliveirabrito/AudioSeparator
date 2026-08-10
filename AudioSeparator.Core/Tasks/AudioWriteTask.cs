using AudioSeparator.Abstractions.Extensions;

namespace AudioSeparator.Core.Tasks;

public class AudioWriteTask(AudioSeparatorContext context) : ProcessTask("Writing audio files")
{
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        context.ModelMetadata.ThrowIfNull();
        ReportProgress(0, context.OutputChunks.Count);

        int stemIndex = 0;
        foreach (var stem in context.OutputChunks)
        {
            var fileName = Path.Combine(Environment.CurrentDirectory, "Outputs", $"{stem.Key}.wav");
            var dirName = Path.GetDirectoryName(fileName);

            if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await context.AudioWriter.WriteAsync(fileName, stem.Value, context.ModelMetadata);
            ReportProgress(++stemIndex, context.OutputChunks.Count);
        }
        ReportProgress(context.OutputChunks.Count, context.OutputChunks.Count);
    }
}