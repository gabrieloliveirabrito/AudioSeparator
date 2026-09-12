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
| `AudioSeparator.Onnx.Mdx` | `AudioSeparator.Onnx.Mdx` | Onnx |
| `AudioSeparator.Benchmark` | `AudioSeparator.Benchmark` | Core |

Examples (`Examples/*`) are never versioned for NuGet.

## Unified version (release train)

All publishable packages share one version from repo-root `Directory.Build.props`:

```xml
<PropertyGroup>
  <Version>0.2.0-beta</Version>
  <PackageVersion>0.2.0-beta</PackageVersion>
  <!-- Authors, RepositoryUrl, PackageLicenseExpression, PackageIcon, … -->
</PropertyGroup>
```

Do **not** set `Version` / `PackageVersion` in individual publishable csproj files. Keep them equal in `Directory.Build.props`.

## When to bump

Apply [SemVer 2.0](https://semver.org/) / NuGet SemVer to the **shared** version for the release train:

| Change type | Bump | Examples |
|-------------|------|----------|
| Breaking public API | **Major** | Remove/rename `UseAudio`, change `SeparationResult` shape |
| New feature, backward compatible | **Minor** | Add overload, new extension method, new publishable package |
| Bug fix, internal refactor | **Patch** | FFMPEG pipe fix, CUDA example fix |

### Prerelease (`-beta`)

NuGet order examples: `0.2.0-beta` &lt; `0.2.0-beta.1` &lt; `0.2.0-beta.2` &lt; `0.2.0`.

- First public train can ship as `0.2.0-beta`.
- Each subsequent merge to `main` while still prerelease must bump (e.g. `0.2.0-beta` → `0.2.0-beta.1`).
- Leaving beta: `0.2.0-beta.N` → `0.2.0`.

## Gate on `main`

CI job `version-gate` (see `.github/workflows/dotnet.yml`) runs on PRs/pushes to `main` and executes:

```bash
bash scripts/check-version-bump.sh
```

The script requires `Directory.Build.props` `Version` to be **strictly greater** than the latest git tag `v*` (NuGet SemVer via `scripts/CompareNuGetVersions.cs`). Equal or lower versions fail the check.

Bootstrap (no tags yet): create the first tag on `main` matching the current props version, e.g. `v0.2.0-beta`, then bump before the next PR into `main`.

PRs into `development` do **not** require a version bump.

## Release flow

1. On `development`, bump `Version` and `PackageVersion` in `Directory.Build.props` (same string).
2. Open PR `development` → `main` (`gh pr create --base main` only for release merges).
3. `version-gate` must pass.
4. After merge, **manually** create a GitHub Release with tag `v{Version}` identical to props (e.g. `v0.2.0-beta.1`).
5. `nuget-publish.yml` runs on `release: published` and packs/pushes all publishable projects (including Onnx.Mdx).

Publish remains release-triggered / `workflow_dispatch`; version on `main` must already be unique vs prior tags to avoid NuGet conflicts.

## New publishable package checklist

When adding a package such as `AudioSeparator.Onnx.Mdx`:

- [ ] Package `README.md` (picked up by `Directory.Build.props` `PackageReadmeFile`)
- [ ] Row + architecture blurb in repo-root `README.md`
- [ ] Entry in `.github/workflows/nuget-publish.yml` pack list
- [ ] Row in this skill’s publishable table
- [ ] Bump shared `Version` / `PackageVersion` if the package ships in the next `main` release

## Checklist (before release PR)

```
- [ ] Bumped Version + PackageVersion in Directory.Build.props (same value)
- [ ] bash scripts/check-version-bump.sh passes against latest v* tags
- [ ] New packages: package README, root README, nuget-publish.yml, this table
- [ ] Examples still use ProjectReference (IsPackable=false where needed)
- [ ] build-all.sh passes
```

## Commands

Current shared version:

```bash
grep -E '<Version>|<PackageVersion>' Directory.Build.props
```

Local gate (needs `git fetch --tags`):

```bash
bash scripts/check-version-bump.sh
```

Pack a single package (from repo root):

```bash
dotnet pack AudioSeparator.Abstractions/AudioSeparator.Abstractions.csproj -c Release -o ./artifacts
```

## Do not

- Bump Examples or `Older/` csproj versions for NuGet.
- Set different `Version` and `PackageVersion` values.
- Put per-package `Version` back into publishable csproj files.
- Merge to `main` without a SemVer bump above the latest `v*` tag.
- Publish without rebuilding after a version bump.
