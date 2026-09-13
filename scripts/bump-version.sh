#!/usr/bin/env bash
# Bump repo-root VERSION in place (prerelease: -beta → -beta.1 → -beta.2; stable → patch).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VERSION_FILE="$ROOT/VERSION"
BUMP_CS="$ROOT/scripts/BumpNuGetVersion.cs"

if [[ ! -f "$VERSION_FILE" ]]; then
  echo "error: VERSION file not found at $VERSION_FILE" >&2
  exit 2
fi

current="$(tr -d '[:space:]' < "$VERSION_FILE")"
if [[ -z "$current" ]]; then
  echo "error: VERSION file is empty" >&2
  exit 2
fi

next="$(dotnet run --verbosity quiet "$BUMP_CS" -- "$current")"
printf '%s\n' "$next" > "$VERSION_FILE"
echo "Bumped VERSION: $current → $next"
