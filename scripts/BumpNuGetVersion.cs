#:package NuGet.Versioning@6.14.0

using NuGet.Versioning;

// Bumps: 0.2.0-beta → 0.2.0-beta.1 → 0.2.0-beta.2; stable 0.2.0 → 0.2.1
if (args.Length != 1 || !NuGetVersion.TryParse(args[0], out var current))
{
    Console.Error.WriteLine("Usage: BumpNuGetVersion.cs <version>");
    return 2;
}

string next;
if (current.IsPrerelease)
{
    var labels = current.ReleaseLabels.ToList();
    if (labels.Count > 0 && int.TryParse(labels[^1], out var n))
        labels[^1] = (n + 1).ToString();
    else
        labels.Add("1");

    next = $"{current.Major}.{current.Minor}.{current.Patch}-{string.Join('.', labels)}";
}
else
{
    next = $"{current.Major}.{current.Minor}.{current.Patch + 1}";
}

Console.Write(next);
return 0;
