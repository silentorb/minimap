# Agent notes — minimap

## Project

- **Engine**: Godot **4.6**, Forward Plus renderer; gameplay is **2D** (single-screen hex arena—see [docs/game/game-design.md](docs/game/game-design.md)).
- **Entry**: `run/main_scene` is `res://scenes/lobby.tscn` for normal play; `res://scenes/world.tscn` remains for direct load (developers, automation). See [project.godot](project.godot).
- **Name / assembly**: Application id is `minimap`; [project.godot](project.godot) sets `[dotnet]` `project/assembly_name` for C# when used.
- **C# modules**:
  - **`Minimap.Simulation.Types`** — shared definition/content types (little/no logic); Simulation and Extensive depend on it.
  - **`Minimap.Simulation`** — authoritative game logic and state (no Godot, no user input/output).
  - **`Minimap.Extensive`** — extension contracts, registry, and default integrator (no Godot, no file I/O).
  - **`Minimap.Client`** — Godot scripts, rendering, input, HUD (sources under `src/Minimap.Client/`); also a class library for tests. Depends on Simulation (minimized; HUD types stay Simulation-free).
  - **`Minimap.App`** — thin composition between Simulation and Client (`GameSession`, `GameApp`); loads extensions from config; mitigates tight coupling.
  - **`CompuQuest.Minimap`** — content extension library (loadable DLL under `extensions/`; not referenced by the Godot host).
  - Root [minimap.csproj](minimap.csproj) is the Godot host and **compiles App + Client scripts into the main assembly** (Godot only resolves C# scripts from that assembly), referencing Simulation + Extensive + Automation.Contracts.

## Layout

Godot-related directories (see [docs/technical/technical-design.md](docs/technical/technical-design.md) **Godot project layout**):

| Path | Purpose |
|------|---------|
| [`assets/`](assets/) | Images, audio, etc. |
| [`entities/`](entities/) | Scenes for elements used inside a root scene |
| [`extensions/`](extensions/) | Built extension DLLs for local load |
| [`scenes/`](scenes/) | Root scenes (e.g. `world.tscn`) |
| [`src/`](src/) | C# (`Minimap.Simulation.Types`, `Minimap.Simulation`, `Minimap.Extensive`, `Minimap.Client`, `Minimap.App`, `CompuQuest.Minimap`, …) |
| [`tests/`](tests/) | Test projects |
| [`ui/`](ui/) | UI scenes and related resources |

Also at repo root: [project.godot](project.godot), [minimap.csproj](minimap.csproj), [icon.svg](icon.svg).

## Conventions

- **Line endings:** Use **Unix (LF)** for all text in this repo. [`.gitattributes`](.gitattributes) enforces `eol=lf` on checkout/commit; [`.editorconfig`](.editorconfig) sets `end_of_line = lf`. The dev container sets **`files.eol`** to `\n` in VS Code / Cursor so new files default to LF. If you create or edit files on Windows outside the setup, set the editor to LF (not CRLF) and avoid reintroducing `\r\n`; use `git add --renormalize .` if you need to fix a batch of files after changing `.gitattributes`.
- Prefer changing game logic and scenes in this repo; keep Godot editor–managed files (`*.tscn`, `project.godot`) consistent with how Godot serializes them.
- Match existing script language and style in the files you touch (GDScript vs C#).

## Environment

- The **dev container** installs **Godot .NET 4.6** (Linux) and sets **`GODOT_BIN`** (see [`.devcontainer/devcontainer.json`](.devcontainer/devcontainer.json)). Use it for headless Godot functional tests.

## Product and engineering docs (source of truth)

[`docs/`](docs/) is the **source of truth for functionality**. Code and tests implement the docs; when they disagree, update code to match docs (and keep docs current when changing behavior).

- [docs/game/game-design.md](docs/game/game-design.md) — Vision, genre, pillars (2D hex, co-op, proc gen, evolving world). Read when changing **gameplay feel, scope, or player count**.
- [docs/technical/technical-design.md](docs/technical/technical-design.md) — Engine, C#, TDD, docs-as-SoT, simulation vs. visual separation, **Godot project layout**. Read when choosing **architecture, tests, or Godot/C# boundaries**.

## Feature documentation (read on demand)

Do **not** preload the whole `docs/` tree for routine tasks. Skim the feature README trigger tables, then read **only** the matching file(s):

- **Game** (design / player-facing rules): [`docs/game/features/README.md`](docs/game/features/README.md)
- **Technical** (architecture / contracts): [`docs/technical/features/README.md`](docs/technical/features/README.md)
- Automated testing (unit vs functional, xUnit, gRPC-based Godot automation, `dotnet test`, in-container `GODOT_BIN` for client smoke): [`docs/technical/features/testing.md`](docs/technical/features/testing.md), layout: [`tests/functional/README.md`](tests/functional/README.md).
