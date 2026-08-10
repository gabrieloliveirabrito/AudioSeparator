using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AudioSeparator.Abstractions.Extensions;

public static class NullabilityExtensions
{
    public static void ThrowIfNull<T>([NotNull] this T? obj, [CallerArgumentExpression(nameof(obj))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
    }
}