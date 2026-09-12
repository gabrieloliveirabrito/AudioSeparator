#!/usr/bin/env bash
# Fails unless Directory.Build.props Version is strictly greater than the latest v* tag
# (NuGet SemVer, including prereleases such as 0.2.0-beta / 0.2.0-beta.1).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROPS="$ROOT/Directory.Build.props"
COMPARE_CS="$ROOT/scripts/CompareNuGetVersions.cs"

if [[ ! -f "$PROPS" ]]; then
  echo "error: Directory.Build.props not found at $PROPS" >&2
  exit 2
fi

version="$(grep -oP '(?<=<Version>)[^<]+' "$PROPS" | head -n1 || true)"
if [[ -z "$version" ]]; then
  echo "error: could not read <Version> from Directory.Build.props" >&2
  exit 2
fi

package_version="$(grep -oP '(?<=<PackageVersion>)[^<]+' "$PROPS" | head -n1 || true)"
if [[ -n "$package_version" && "$package_version" != "$version" ]]; then
  echo "error: Version ($version) and PackageVersion ($package_version) must match in Directory.Build.props" >&2
  exit 2
fi

# Collect SemVer tags: v1.2.3, v1.2.3-beta, v1.2.3-beta.1
mapfile -t tags < <(git -C "$ROOT" tag -l 'v*' | sed -nE 's/^v([0-9]+\.[0-9]+\.[0-9]+([.-][0-9A-Za-z.-]+)?)$/\1/p' || true)

if [[ ${#tags[@]} -eq 0 ]]; then
  echo "error: no SemVer tags matching v* found." >&2
  echo "Create the first release tag on main aligned to Directory.Build.props, e.g.:" >&2
  echo "  git tag v${version} && git push origin v${version}" >&2
  echo "Or publish a GitHub Release named v${version}. Subsequent PRs to main must bump Version above that tag." >&2
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

echo "Directory.Build.props Version: $version"
echo "Latest release tag:              v${latest_tagged}"

if dotnet run --verbosity quiet "$COMPARE_CS" -- "$version" "$latest_tagged"; then
  echo "OK: $version > $latest_tagged (NuGet SemVer)."
  exit 0
fi

echo "error: Version must be strictly greater than the latest tag before merging to main." >&2
echo "Bump Version and PackageVersion in Directory.Build.props (e.g. ${latest_tagged} → ${latest_tagged%-*}-beta.1 or next stable)." >&2
echo "Examples: 0.2.0-beta → 0.2.0-beta.1 or 0.2.0; 0.2.0 → 0.2.1 / 0.3.0 / 1.0.0." >&2
exit 1
