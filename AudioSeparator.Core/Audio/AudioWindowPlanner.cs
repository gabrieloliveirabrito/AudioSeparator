namespace AudioSeparator.Core.Audio;

public static class AudioWindowPlanner
{
    public static IReadOnlyList<int> ComputeOffsets(
        long totalFrames,
        int segmentLength,
        bool enableOverlap,
        float overlapRatio)
    {
        if (segmentLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentLength));
        }

        if (totalFrames <= 0)
        {
            return [];
        }

        var stride = enableOverlap
            ? Math.Max(1, (int)((1d - overlapRatio) * segmentLength))
            : segmentLength;

        var offsets = new List<int>();
        for (var offset = 0; offset < totalFrames; offset += stride)
        {
            offsets.Add(offset);
        }

        return offsets;
    }
}
