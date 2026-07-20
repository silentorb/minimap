#!/usr/bin/env bash
# Run Minimap.Functional.Godot.Tests (xUnit + gRPC playbook automation).
# Prerequisites:
#   - GODOT_BIN: path to a Godot 4.x .NET executable this environment can execute.
#     In the dev container this is set automatically (see .devcontainer/devcontainer.json).
#   - Optional: MINIMAP_AUTOMATION_PORT (default: auto-picked by tests).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ -z "${GODOT_BIN:-}" ]]; then
  echo "GODOT_BIN is not set. Example: export GODOT_BIN=/opt/godot/Godot_v4.6-stable_mono_linux.x86_64" >&2
  exit 2
fi

# Helpers + playbook libraries must exist before Godot loads them at runtime.
dotnet build "${ROOT}/src/Minimap.Automation/Minimap.Automation.csproj" -v q
dotnet build "${ROOT}/tests/functional/Minimap.Functional.Godot.Playbooks/Minimap.Functional.Godot.Playbooks.csproj" -v q

# Ensure Godot's C# assemblies (including [ScriptPath] metadata) are up to date.
dotnet build "${ROOT}/minimap.csproj" -v q

dotnet test "${ROOT}/tests/functional/Minimap.Functional.Godot.Tests/Minimap.Functional.Godot.Tests.csproj" \
  "$@"
