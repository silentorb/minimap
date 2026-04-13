# Remote headless Godot (WSL launcher)

To run **headless Windows Godot** from this dev container, use the narrow HTTP launcher in [`tools/godot-launcher/`](../../tools/godot-launcher/). **Build it** with [`scripts/build_godot_launcher.sh`](../../scripts/build_godot_launcher.sh) from the repo root (requires Rust in the dev container; see [`.devcontainer/Dockerfile`](../../.devcontainer/Dockerfile)). You still typically **run** `dist/tools/godot-launcher` in **WSL** so it can spawn Windows `Godot.exe`. Agents call [`scripts/godot_remote.py`](../../scripts/godot_remote.py) (`python3`).

The dev container **does not** set `GODOT_REMOTE_URL` or `GODOT_REMOTE_TOKEN`. To use the client from inside the container, **opt in** in a shell session, for example:

```bash
export GODOT_REMOTE_URL=http://127.0.0.1:27182
export GODOT_REMOTE_TOKEN=<same value as GODOT_LAUNCHER_TOKEN where godot-launcher runs>
```

Then run `python3 scripts/godot_remote.py` (or your wrapper) as needed.

If you prefer **host passthrough** (set the token once on the machine that starts the dev container), add `GODOT_REMOTE_URL` and `GODOT_REMOTE_TOKEN` (e.g. `${localEnv:GODOT_REMOTE_TOKEN}`) under `containerEnv` in [`.devcontainer/devcontainer.json`](../../.devcontainer/devcontainer.json) **only on your machine**—do **not** commit real tokens.

Do **not** commit tokens. See [`tools/godot-launcher/README.md`](../../tools/godot-launcher/README.md) for API and security notes.
