# Agent notes — minimap

## Project

- **Engine**: Godot **4.6**, Forward Plus renderer; gameplay is **2D** (single-screen hex arena—see [docs/game/game-design.md](docs/game/game-design.md)).
- **Entry**: `run/main_scene` is `res://scenes/world.tscn` (see [project.godot](project.godot)).
- **Name / assembly**: Application id is `minimap`; [project.godot](project.godot) sets `[dotnet]` `project/assembly_name` for C# when used.
- **C# modules**: **`Minimap.Simulation`** — authoritative game logic and state (no Godot references). **`Minimap.Client`** — Godot scripts, rendering, input; references Simulation. Root [minimap.csproj](minimap.csproj) is the Godot host and references **Client** only.

## Layout

Godot-related directories (see [docs/technical/technical-design.md](docs/technical/technical-design.md) **Godot project layout**):

| Path | Purpose |
|------|---------|
| [`assets/`](assets/) | Images, audio, etc. |
| [`entities/`](entities/) | Scenes for elements used inside a root scene |
| [`scenes/`](scenes/) | Root scenes (e.g. `world.tscn`) |
| [`src/`](src/) | C# (`Minimap.Simulation`, `Minimap.Client`) |
| [`tests/`](tests/) | Test projects |
| [`ui/`](ui/) | UI scenes and related resources |

Also at repo root: [project.godot](project.godot), [minimap.csproj](minimap.csproj), [icon.svg](icon.svg).

## Conventions

- **Line endings:** Use **Unix (LF)** for all text in this repo. [`.gitattributes`](.gitattributes) enforces `eol=lf` on checkout/commit; [`.editorconfig`](.editorconfig) sets `end_of_line = lf`. The dev container sets **`files.eol`** to `\n` in VS Code / Cursor so new files default to LF. If you create or edit files on Windows outside that setup, set the editor to LF (not CRLF) and avoid reintroducing `\r\n`; use `git add --renormalize .` if you need to fix a batch of files after changing `.gitattributes`.
- Prefer changing game logic and scenes in this repo; keep Godot editor–managed files (`*.tscn`, `project.godot`) consistent with how Godot serializes them.
- Match existing script language and style in the files you touch (GDScript vs C#).
- **`godot-launcher` (Rust):** From the **repository root**, the default **wrapper build** is **`./scripts/build_godot_launcher.sh`**: it runs `cargo build --release` for the crate and **also copies** the release binary to **`dist/tools/`** (gitignored) for a stable path outside Cargo’s output tree. In the **dev container**, **`CARGO_TARGET_DIR`** is **`/workspaces/build/cargo-target`** (see [`.devcontainer/devcontainer.json`](.devcontainer/devcontainer.json)) so artifacts live under **`/workspaces/build/`**, not under the bind-mounted repo; **`postCreateCommand`** runs the wrapper once after the container is created. Use `cd tools/godot-launcher && cargo test` (or `cargo build`) only when you need crate-scoped compile/test iteration; do **not** treat a bare `cargo build --release` in that directory as the standard way to produce the launcher artifact for this repo.

## Environment

- Godot is not in the current dev container environment and exists outside of it in Windows.

## Product and engineering docs

- [docs/game/game-design.md](docs/game/game-design.md) — Vision, genre, pillars (2D hex, co-op, proc gen, evolving world). Read when changing **gameplay feel, scope, or player count**.
- [docs/technical/technical-design.md](docs/technical/technical-design.md) — Engine, C#, TDD, docs under `./docs`, simulation vs. visual separation, **Godot project layout**. Read when choosing **architecture, tests, or Godot/C# boundaries**.

## Extended documentation (read on demand)

- [`docs/`](docs/) holds **optional** notes for agents and humans. Do **not** preload the whole tree for routine tasks.
- Feature-specific guides: **game** tasks → [`docs/game/features/README.md`](docs/game/features/README.md); **technical** tasks → [`docs/technical/features/README.md`](docs/technical/features/README.md). Match your task to a trigger there, then read **only** the listed file(s).
- Automated testing (unit vs functional, xUnit, gRPC-based Godot automation, `dotnet test`, `GODOT_BIN` for client smoke): [`docs/technical/features/testing.md`](docs/technical/features/testing.md), layout: [`tests/functional/README.md`](tests/functional/README.md).
- Remote / headless Godot from the dev container (WSL launcher, `godot_remote.py`; **`GODOT_REMOTE_URL`** is set in [`devcontainer.json`](.devcontainer/devcontainer.json)): [`docs/technical/features/remote-headless-godot.md`](docs/technical/features/remote-headless-godot.md).
