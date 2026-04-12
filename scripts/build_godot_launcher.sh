#!/usr/bin/env bash
# Wrapper around `cargo build --release` for tools/godot-launcher; also copies the binary to
# ./dist/tools/ (gitignored). In the dev container, CARGO_TARGET_DIR is set to /workspaces/build/
# so Cargo output is ephemeral (not under the bind-mounted repo). See AGENTS.md.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CRATE="${ROOT}/tools/godot-launcher"
OUT="${ROOT}/dist/tools"
TARGET_ROOT="${CARGO_TARGET_DIR:-${CRATE}/target}"

mkdir -p "${OUT}"
(cd "${CRATE}" && cargo build --release)

unix_bin="${TARGET_ROOT}/release/godot-launcher"
win_bin="${TARGET_ROOT}/release/godot-launcher.exe"

if [[ -f "${unix_bin}" ]]; then
  cp -f "${unix_bin}" "${OUT}/godot-launcher"
  echo "Wrote ${OUT}/godot-launcher"
elif [[ -f "${win_bin}" ]]; then
  cp -f "${win_bin}" "${OUT}/godot-launcher.exe"
  echo "Wrote ${OUT}/godot-launcher.exe"
else
  echo "error: release binary not found at ${unix_bin} or ${win_bin}" >&2
  exit 1
fi
