# Agent notes — minimap

## Project

- **Engine**: Godot **4.6**, Forward Plus renderer; gameplay is **2D** (single-screen hex arena—see [docs/game/game-design.md](docs/game/game-design.md)).
- **Entry**: `run/main_scene` is `res://scenes/lobby.tscn` for normal play; `res://scenes/world.tscn` remains for direct load (developers, automation). See [project.godot](project.godot).
- **Name / assembly**: Application id is `minimap`; [project.godot](project.godot) sets `[dotnet]` `project/assembly_name` for C# when used.
- **C# modules**:
  - **`Minimap.Simulation.Types`** — shared contracts only; see [`src/Minimap.Simulation.Types/AGENTS.md`](src/Minimap.Simulation.Types/AGENTS.md).
  - **`Minimap.Simulation`** — authoritative game logic and state; see [`src/Minimap.Simulation/AGENTS.md`](src/Minimap.Simulation/AGENTS.md).
  - **`Minimap.Simulation.Navigation`** — Godot-backed navmesh / crowd steering; see [`src/Minimap.Simulation.Navigation/AGENTS.md`](src/Minimap.Simulation.Navigation/AGENTS.md).
  - **`Minimap.Extensive`** — extension contracts, registry, and default integrator; see [`src/Minimap.Extensive/AGENTS.md`](src/Minimap.Extensive/AGENTS.md).
  - **`Minimap.Client`** — Godot scripts, rendering, input, HUD; see [`src/Minimap.Client/AGENTS.md`](src/Minimap.Client/AGENTS.md).
  - **`Minimap.App`** — thin composition between Simulation and Client; see [`src/Minimap.App/AGENTS.md`](src/Minimap.App/AGENTS.md).
  - **`CompuQuest.Minimap`** — content extension library; see [`src/CompuQuest.Minimap/AGENTS.md`](src/CompuQuest.Minimap/AGENTS.md).
  - **`Minimap.Automation.Contracts`** — automation protobuf/gRPC and playbook interfaces; see [`src/Minimap.Automation.Contracts/AGENTS.md`](src/Minimap.Automation.Contracts/AGENTS.md).
  - **`Minimap.Automation`** — in-process Godot automation helpers; see [`src/Minimap.Automation/AGENTS.md`](src/Minimap.Automation/AGENTS.md).
  - Root [minimap.csproj](minimap.csproj) is the Godot host and **compiles App + Client scripts into the main assembly** (Godot only resolves C# scripts from that assembly), referencing Simulation + Simulation.Navigation + Extensive + Automation.Contracts.

## Layout

Godot-related directories (see [docs/technical/technical-design.md](docs/technical/technical-design.md) **Godot project layout**):

| Path | Purpose |
|------|---------|
| [`assets/`](assets/) | Images, audio, etc. |
| [`entities/`](entities/) | Scenes for elements used inside a root scene |
| [`extensions/`](extensions/) | Built extension DLLs for local load |
| [`scenes/`](scenes/) | Root scenes (e.g. `world.tscn`) |
| [`src/`](src/) | C# (`Minimap.Simulation.Types`, `Minimap.Simulation`, `Minimap.Simulation.Navigation`, `Minimap.Extensive`, `Minimap.Client`, `Minimap.App`, `CompuQuest.Minimap`, …) |
| [`tests/`](tests/) | Test projects |
| [`ui/`](ui/) | UI scenes and related resources |

Also at repo root: [project.godot](project.godot), [minimap.csproj](minimap.csproj), [icon.svg](icon.svg).

## Conventions

- **Line endings:** Use **Unix (LF)** for all text in this repo. [`.gitattributes`](.gitattributes) enforces `eol=lf` on checkout/commit; [`.editorconfig`](.editorconfig) sets `end_of_line = lf`. The dev container sets **`files.eol`** to `\n` in VS Code / Cursor so new files default to LF. If you create or edit files on Windows outside the setup, set the editor to LF (not CRLF) and avoid reintroducing `\r\n`; use `git add --renormalize .` if you need to fix a batch of files after changing `.gitattributes`.
- Prefer changing game logic and scenes in this repo; keep Godot editor–managed files (`*.tscn`, `project.godot`) consistent with how Godot serializes them.
- Match existing script language and style in the files you touch (GDScript vs C#).
- **Bug regressions:** When fixing a user-reported bug the suite missed, add a regression test at the lowest sound layer—or escalate instead of brittle/flaky coverage. See [`.cursor/rules/bug-regression-tests.mdc`](.cursor/rules/bug-regression-tests.mdc) and [docs/technical/features/testing.md](docs/technical/features/testing.md) (**Bug regressions / debugging**).
- **Error handling:** Prefer explicit outcomes for expected failures; use exceptions only for truly exceptional cases or documented fail-fast abort boundaries. Non-trivial paths need a deliberate failure strategy. See [`.cursor/rules/error-handling.mdc`](.cursor/rules/error-handling.mdc) and [docs/technical/features/error-handling.md](docs/technical/features/error-handling.md).

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
- Automated testing (unit vs functional, xUnit, gRPC-based Godot automation, `dotnet test`, in-container `GODOT_BIN` for client smoke, bug regressions): [`docs/technical/features/testing.md`](docs/technical/features/testing.md), layout: [`tests/functional/README.md`](tests/functional/README.md).
