#:package NuGet.Versioning@6.14.0

using NuGet.Versioning;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: CompareNuGetVersions.cs <current> <tagged>");
    return 2;
}

if (!NuGetVersion.TryParse(args[0], out var current))
{
    Console.Error.WriteLine($"Invalid NuGet version: '{args[0]}'");
    return 2;
}

if (!NuGetVersion.TryParse(args[1], out var tagged))
{
    Console.Error.WriteLine($"Invalid NuGet version: '{args[1]}'");
    return 2;
}

// Exit 0 when current > tagged; 1 when equal or lower.
return current > tagged ? 0 : 1;
