# Agent notes — minimap

## Project

- **Engine**: Godot **4.6**, Forward Plus renderer, **Jolt** for 3D physics.
- **Entry**: `run/main_scene` is `res://main.tscn` (see `project.godot`).
- **Name / assembly**: Application id is `minimap`; `project.godot` sets `[dotnet]` `project/assembly_name` for C# when used.

## Layout

- Primary game content lives at the repo root (`project.godot`, scenes, scripts, assets).

## Conventions

- **Line endings:** Use **Unix (LF)** for all text in this repo. [`.gitattributes`](.gitattributes) enforces `eol=lf` on checkout/commit; [`.editorconfig`](.editorconfig) sets `end_of_line = lf`. The dev container sets **`files.eol`** to `\n` in VS Code / Cursor so new files default to LF. If you create or edit files on Windows outside that setup, set the editor to LF (not CRLF) and avoid reintroducing `\r\n`; use `git add --renormalize .` if you need to fix a batch of files after changing `.gitattributes`.
- Prefer changing game logic and scenes in this repo; keep Godot editor–managed files (`*.tscn`, `project.godot`) consistent with how Godot serializes them.
- Match existing script language and style in the files you touch (GDScript vs C#).
- **`godot-launcher` (Rust):** From the **repository root**, the default **wrapper build** is **`./scripts/build_godot_launcher.sh`**: it runs `cargo build --release` for the crate and **also copies** the release binary to **`dist/tools/`** (gitignored) for a stable path outside Cargo’s output tree. In the **dev container**, **`CARGO_TARGET_DIR`** is **`/workspaces/build/cargo-target`** (see [`.devcontainer/devcontainer.json`](.devcontainer/devcontainer.json)) so artifacts live under **`/workspaces/build/`**, not under the bind-mounted repo; **`postCreateCommand`** runs the wrapper once after the container is created. Use `cd tools/godot-launcher && cargo test` (or `cargo build`) only when you need crate-scoped compile/test iteration; do **not** treat a bare `cargo build --release` in that directory as the standard way to produce the launcher artifact for this repo.

## Environment

- Godot is not in the current dev container environment and exists outside of it in Windows.

## Extended documentation (read on demand)

- [`docs/`](docs/) holds **optional** notes for agents and humans. Do **not** preload the whole tree for routine tasks.
- Feature-specific guides live under [`docs/features/`](docs/features/). Use [`docs/features/README.md`](docs/features/README.md) as a **catalog**: match your task to a trigger there, then read **only** the listed file(s).
- Remote / headless Godot from the dev container (WSL launcher, `godot_remote.py`, `GODOT_REMOTE_*`): see [`docs/features/remote-headless-godot.md`](docs/features/remote-headless-godot.md).
