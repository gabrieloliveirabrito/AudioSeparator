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

Source of truth is the repo-root [`VERSION`](VERSION) file. `Directory.Build.props` reads it into `Version` / `PackageVersion` for all projects.

```text
VERSION                 →  0.2.0-beta.1
Directory.Build.props   →  reads VERSION (do not hardcode Version here)
```

Do **not** set `Version` / `PackageVersion` in individual publishable csproj files.

## When to bump

Apply [SemVer 2.0](https://semver.org/) / NuGet SemVer to **VERSION**:

| Change type | Bump | Examples |
|-------------|------|----------|
| Breaking public API | **Major** | Remove/rename `UseAudio`, change `SeparationResult` shape |
| New feature, backward compatible | **Minor** | Add overload, new extension method, new publishable package |
| Bug fix, internal refactor | **Patch** | FFMPEG pipe fix, CUDA example fix |

### Prerelease (`-beta`) and auto-bump

NuGet order: `0.2.0-beta` &lt; `0.2.0-beta.1` &lt; `0.2.0-beta.2` &lt; `0.2.0`.

Auto-bump (`bash scripts/bump-version.sh` / main pre-push):

- Prerelease: `0.2.0-beta` → `0.2.0-beta.1` → `0.2.0-beta.2`
- Stable: `0.2.0` → `0.2.1`

Leaving beta manually: set `VERSION` to `0.2.0` (or next minor/major).

## Gate on `main`

CI job `version-gate` runs on PRs/pushes to `main`:

```bash
bash scripts/check-version-bump.sh
```

Requires root `VERSION` to be **strictly greater** than the latest git tag `v*`.

PRs into `development` do **not** require a version bump.

## Pre-push hook (direct pushes to `main`)

Repo hooks live in [`.githooks/`](.githooks/). Enable once per clone:

```bash
git config core.hooksPath .githooks
```

On `git push` targeting `refs/heads/main`:

- If `VERSION` **is** in the commits being pushed → no auto-bump; run the SemVer gate.
- If `VERSION` **is not** in those commits → run `scripts/bump-version.sh`, abort the push, and ask you to commit `VERSION` and push again.

GitHub PR merges into `main` do **not** run this hook — edit `VERSION` (or run `bump-version.sh`) on `development` before the release PR. CI still enforces the gate.

## Release flow

1. On `development`, set `VERSION` above the latest `v*` tag (edit by hand or `bash scripts/bump-version.sh`).
2. Open PR `development` → `main`.
3. `version-gate` must pass.
4. After merge, create a GitHub Release with tag `v{VERSION}` (e.g. `v0.2.0-beta.2`).
5. `nuget-publish.yml` packs/pushes all publishable projects (including Onnx.Mdx).

## New publishable package checklist

- [ ] Package `README.md` (via `Directory.Build.props` `PackageReadmeFile`)
- [ ] Row + architecture blurb in repo-root `README.md`
- [ ] Entry in `.github/workflows/nuget-publish.yml` pack list
- [ ] Row in this skill’s publishable table
- [ ] Bump `VERSION` if the package ships in the next `main` release

## Checklist (before release PR)

```
- [ ] VERSION bumped above latest v* tag
- [ ] bash scripts/check-version-bump.sh passes
- [ ] New packages: package README, root README, nuget-publish.yml, this table
- [ ] Examples still use ProjectReference (IsPackable=false where needed)
- [ ] build-all.sh passes
- [ ] git config core.hooksPath .githooks (local clones that push to main)
```

## Commands

```bash
cat VERSION
bash scripts/bump-version.sh
bash scripts/check-version-bump.sh
dotnet pack AudioSeparator.Abstractions/AudioSeparator.Abstractions.csproj -c Release -o ./artifacts
```

## Do not

- Bump Examples or `Older/` for NuGet.
- Hardcode `Version` / `PackageVersion` in publishable csproj or `Directory.Build.props`.
- Merge to `main` without `VERSION` above the latest `v*` tag.
- Publish without rebuilding after a version bump.
