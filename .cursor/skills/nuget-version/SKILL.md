---
name: nuget-version
description: >-
  Bump and align NuGet package versions across AudioSeparator csproj projects
  before publish. Use when releasing packages, updating dependencies, changing
  public API, or when the user mentions NuGet version, semver, or csproj version.
---

# NuGet version management (AudioSeparator)

## Publishable packages

Only these projects are published to NuGet (not Examples or Older):

| Project | PackageId (default) | Depends on |
|---------|-------------------|------------|
| `AudioSeparator.Abstractions` | `AudioSeparator.Abstractions` | — |
| `AudioSeparator.Core` | `AudioSeparator.Core` | Abstractions |
| `AudioSeparator.FFMPEG` | `AudioSeparator.FFMPEG` | Abstractions |
| `AudioSeparator.NAudio` | `AudioSeparator.NAudio` | Abstractions |
| `AudioSeparator.Onnx` | `AudioSeparator.Onnx` | Core |
| `AudioSeparator.Onnx.Demucs` | `AudioSeparator.Onnx.Demucs` | Onnx |

Examples (`Examples/*`) are never versioned for NuGet.

## Version properties

Each publishable `.csproj` should expose at minimum:

```xml
<PropertyGroup>
  <Version>0.1.0</Version>
  <PackageVersion>0.1.0</PackageVersion>
  <Authors>…</Authors>
  <Description>…</Description>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/gabrieloliveirabrito/AudioSeparator</RepositoryUrl>
</PropertyGroup>
```

Prefer a repo-root `Directory.Build.props` for shared metadata (`Authors`, `RepositoryUrl`, `PackageLicenseExpression`) and keep `Version` / `PackageVersion` in each publishable csproj until a unified release train is adopted.

## When to bump

Apply [SemVer 2.0](https://semver.org/) per **changed** package:

| Change type | Bump | Examples |
|-------------|------|----------|
| Breaking public API | **Major** | Remove/rename `UseAudio`, change `SeparationResult` shape |
| New feature, backward compatible | **Minor** | Add overload, new extension method |
| Bug fix, internal refactor | **Patch** | FFMPEG pipe fix, CUDA example fix |

If package A depends on B and B gets a **major** bump, bump A at least at the same major (and update `PackageReference` / project refs if packages are consumed via NuGet instead of ProjectReference).

## Workflow before publish

1. **Identify changed packages** — `git diff --name-only` mapped to csproj roots above.
2. **Bump only affected publishable csproj** — increment `Version` and `PackageVersion` together (same string).
3. **Align dependency versions** — when internal packages are referenced as NuGet (not ProjectReference), update `PackageReference` version to match the bumped dependency.
4. **Build** — `sh build-all.sh` (or `dotnet build` on changed projects).
5. **Verify** — no duplicate/conflicting versions across the dependency graph.

## Checklist

```
- [ ] Listed which publishable packages changed
- [ ] Bumped Version + PackageVersion on each changed package
- [ ] Updated downstream PackageReference versions (if applicable)
- [ ] Examples still use ProjectReference (no accidental publish)
- [ ] build-all.sh passes
```

## Commands

Find current versions (once properties exist):

```bash
rg '<Version>|<PackageVersion>' AudioSeparator.*/AudioSeparator.*.csproj
```

Publish a single package (from repo root):

```bash
dotnet pack AudioSeparator.Abstractions/AudioSeparator.Abstractions.csproj -c Release -o ./artifacts
```

## Do not

- Bump Examples or `Older/` csproj versions for NuGet.
- Mix `Version` and `PackageVersion` with different values on the same project.
- Publish without rebuilding after a version bump.
