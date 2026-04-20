#!/usr/bin/env bash
# Run Minimap.Functional.Godot.Tests (xUnit + gRPC automation).
# Prerequisites:
#   - GODOT_BIN: path to a Godot 4.x executable this environment can execute.
#   - Optional: MINIMAP_AUTOMATION_PORT (default: auto-picked by tests).
#   - Optional: GODOT_REMOTE_URL to health-check tools/godot-launcher.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ -n "${GODOT_REMOTE_URL:-}" ]]; then
  python3 "${ROOT}/scripts/godot_remote.py" health
fi

if [[ -z "${GODOT_BIN:-}" ]]; then
  echo "GODOT_BIN is not set. Example (WSL with Windows Godot): export GODOT_BIN=/mnt/c/Apps/Godot/Godot_v4.6-stable_win64.exe" >&2
  exit 2
fi

dotnet test "${ROOT}/tests/functional/Minimap.Functional.Godot.Tests/Minimap.Functional.Godot.Tests.csproj" \
  "$@"
