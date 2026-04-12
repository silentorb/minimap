# Agent notes — marloth

## Project

- **Engine**: Godot **4.6**, Forward Plus renderer, **Jolt** for 3D physics.
- **Entry**: `run/main_scene` is `res://main.tscn` (see `project.godot`).
- **Name / assembly**: Application id is `marloth`; `project.godot` sets `[dotnet]` `project/assembly_name` for C# when used.

## Layout

- Primary game content lives at the repo root (`project.godot`, scenes, scripts, assets).
- **`.mnt/unreal/marloth/`** is a **read-only** bind of external Unreal source (see `.devcontainer/devcontainer.json`). Treat it as reference or legacy context unless the task explicitly concerns it; do not assume it is built or edited as part of this Godot tree.

## Conventions

- **Line endings:** Use **Unix (LF)** for all text in this repo. [`.gitattributes`](.gitattributes) enforces `eol=lf` on checkout/commit; [`.editorconfig`](.editorconfig) sets `end_of_line = lf`. The dev container sets **`files.eol`** to `\n` in VS Code / Cursor so new files default to LF. If you create or edit files on Windows outside that setup, set the editor to LF (not CRLF) and avoid reintroducing `\r\n`; use `git add --renormalize .` if you need to fix a batch of files after changing `.gitattributes`.
- Prefer changing game logic and scenes in this repo; keep Godot editor–managed files (`*.tscn`, `project.godot`) consistent with how Godot serializes them.
- Match existing script language and style in the files you touch (GDScript vs C#).
- **`godot-launcher` (Rust):** From the **repository root**, the default **wrapper build** is **`./scripts/build_godot_launcher.sh`**: it runs `cargo build --release` for the crate and **also copies** the release binary to **`dist/tools/`** (gitignored) for a stable path outside Cargo’s output tree. In the **dev container**, **`CARGO_TARGET_DIR`** is **`/workspaces/build/cargo-target`** (see [`.devcontainer/devcontainer.json`](.devcontainer/devcontainer.json)) so artifacts live under **`/workspaces/build/`**, not under the bind-mounted repo; **`postCreateCommand`** runs the wrapper once after the container is created. Use `cd tools/godot-launcher && cargo test` (or `cargo build`) only when you need crate-scoped compile/test iteration; do **not** treat a bare `cargo build --release` in that directory as the standard way to produce the launcher artifact for this repo.

## Environment

- Godot is not in the current dev container environment and exists outside of it in Windows.

## Remote headless Godot (WSL launcher)

To run **headless Windows Godot** from this dev container, use the narrow HTTP launcher in [`tools/godot-launcher/`](tools/godot-launcher/). **Build it** with [`scripts/build_godot_launcher.sh`](scripts/build_godot_launcher.sh) from the repo root (requires Rust in the dev container; see [`.devcontainer/Dockerfile`](.devcontainer/Dockerfile)). You still typically **run** `dist/tools/godot-launcher` in **WSL** so it can spawn Windows `Godot.exe`. Agents call [`scripts/godot_remote.py`](scripts/godot_remote.py) (`python3`).

- Set **`GODOT_REMOTE_TOKEN`** on the **host** (same value as `GODOT_LAUNCHER_TOKEN` in WSL). The dev container receives it via `containerEnv` substitution from `localEnv` (see [`.devcontainer/devcontainer.json`](.devcontainer/devcontainer.json)).
- **`GODOT_REMOTE_URL`** defaults to `http://127.0.0.1:27182`; adjust if your launcher listens elsewhere.
- Do **not** commit tokens. See [`tools/godot-launcher/README.md`](tools/godot-launcher/README.md) for API and security notes.
