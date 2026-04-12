# Remote headless Godot (WSL launcher)

To run **headless Windows Godot** from this dev container, use the narrow HTTP launcher in [`tools/godot-launcher/`](../../tools/godot-launcher/). **Build it** with [`scripts/build_godot_launcher.sh`](../../scripts/build_godot_launcher.sh) from the repo root (requires Rust in the dev container; see [`.devcontainer/Dockerfile`](../../.devcontainer/Dockerfile)). You still typically **run** `dist/tools/godot-launcher` in **WSL** so it can spawn Windows `Godot.exe`. Agents call [`scripts/godot_remote.py`](../../scripts/godot_remote.py) (`python3`).

- Set **`GODOT_REMOTE_TOKEN`** on the **host** (same value as `GODOT_LAUNCHER_TOKEN` in WSL). The dev container receives it via `containerEnv` substitution from `localEnv` (see [`.devcontainer/devcontainer.json`](../../.devcontainer/devcontainer.json)).
- **`GODOT_REMOTE_URL`** defaults to `http://127.0.0.1:27182`; adjust if your launcher listens elsewhere.
- Do **not** commit tokens. See [`tools/godot-launcher/README.md`](../../tools/godot-launcher/README.md) for API and security notes.
