namespace AudioSeparator.Abstractions;

/// <summary>
/// Strategies for avoiding raw clipping on output stems (Demucs prevent_clip).
/// </summary>
public enum ClipPreventMode
{
    None = 0,
    Rescale = 1,
    Clamp = 2,
    Tanh = 3
}
