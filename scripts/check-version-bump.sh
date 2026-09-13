#!/usr/bin/env bash
# Fails unless root VERSION is strictly greater than the latest v* tag
# (NuGet SemVer, including prereleases such as 0.2.0-beta / 0.2.0-beta.1).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VERSION_FILE="$ROOT/VERSION"
COMPARE_CS="$ROOT/scripts/CompareNuGetVersions.cs"

if [[ ! -f "$VERSION_FILE" ]]; then
  echo "error: VERSION file not found at $VERSION_FILE" >&2
  exit 2
fi

version="$(tr -d '[:space:]' < "$VERSION_FILE")"
if [[ -z "$version" ]]; then
  echo "error: VERSION file is empty" >&2
  exit 2
fi

# Collect SemVer tags: v1.2.3, v1.2.3-beta, v1.2.3-beta.1
mapfile -t tags < <(git -C "$ROOT" tag -l 'v*' | sed -nE 's/^v([0-9]+\.[0-9]+\.[0-9]+([.-][0-9A-Za-z.-]+)?)$/\1/p' || true)

if [[ ${#tags[@]} -eq 0 ]]; then
  echo "error: no SemVer tags matching v* found." >&2
  echo "Create the first release tag on main aligned to VERSION, e.g.:" >&2
  echo "  git tag v${version} && git push origin v${version}" >&2
  echo "Or publish a GitHub Release named v${version}. Subsequent PRs to main must bump VERSION above that tag." >&2
  exit 1
fi

latest_tagged=""
for tag_ver in "${tags[@]}"; do
  if [[ -z "$latest_tagged" ]]; then
    latest_tagged="$tag_ver"
    continue
  fi
  if dotnet run --verbosity quiet "$COMPARE_CS" -- "$tag_ver" "$latest_tagged"; then
    latest_tagged="$tag_ver"
  fi
done

echo "VERSION:            $version"
echo "Latest release tag: v${latest_tagged}"

if dotnet run --verbosity quiet "$COMPARE_CS" -- "$version" "$latest_tagged"; then
  echo "OK: $version > $latest_tagged (NuGet SemVer)."
  exit 0
fi

echo "error: VERSION must be strictly greater than the latest tag before merging to main." >&2
echo "Edit VERSION or run: bash scripts/bump-version.sh" >&2
echo "Examples: 0.2.0-beta → 0.2.0-beta.1; 0.2.0-beta.1 → 0.2.0-beta.2; 0.2.0 → 0.2.1." >&2
exit 1
