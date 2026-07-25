# Minimap technical design

# High-level requirements

- **Minimap** is the higher-level game engine built on **Godot**; **CompuQuest** is the surface game (content extension) built on Minimap. Godot remains the underlying engine/runtime.
- C# is the primary programming language
- Mostly developed by AI agents
- Heavily requirements-driven, documented under the `./docs` directory. **`./docs` is the source of truth for functionality**: CompuQuest vision/pillars in `docs/game/game-design.md` (agents must not edit that file without explicit user instruction); secondary game rules under `docs/game/features/`; architecture and contracts under `docs/technical/`. Code and tests implement those documents. When behavior changes, update the docs in the same change (or first). If code and docs disagree, docs win and code is fixed.
- Heavily test-driven, using both unit tests and functional tests. Tests verify **documented** requirements (values and rules stated in feature docs), not undocumented code quirks. User-reported gaps that the suite missed get a regression test when a sound one exists at the lowest practical layer; otherwise escalate rather than adding brittle or flaky coverage (see [features/platform/testing.md](features/platform/testing.md) **Bug regressions / debugging**).
- Prefer **explicit error outcomes** for expected failures; use **exceptions** only for truly exceptional cases or documented fail-fast abort boundaries (see [features/platform/error-handling.md](features/platform/error-handling.md)).
- No global state, except where needed for integration with Godot and third-party libraries
- Clean separation between visual game state and simulation game state
- **C# project boundaries** (details in each project’s `AGENTS.md`):
  - **Minimap.Simulation.Types** — shared contracts only; see `src/Minimap.Simulation.Types/AGENTS.md`
  - **Minimap.Simulation** — authoritative logic/state; see `src/Minimap.Simulation/AGENTS.md`
  - **Minimap.Simulation.Navigation** — Godot-backed navmesh / crowd steering implementing Simulation navigation interfaces; see `src/Minimap.Simulation.Navigation/AGENTS.md`
  - **Minimap.Extensive** — extension contracts, registry, and default integrator; see `src/Minimap.Extensive/AGENTS.md`
  - **Minimap.Client** — Godot rendering, input capture, HUD; see `src/Minimap.Client/AGENTS.md`
  - **Minimap.App** — settings/extension file I/O and host hooks (not a scene root); see `src/Minimap.App/AGENTS.md`
  - **CompuQuest.Minimap** — CompuQuest surface-game content extension; see `src/CompuQuest.Minimap/AGENTS.md`

# Godot project layout

Below are the directories used to store Godot-related files.

It is not an exhaustive list of all the directories in this project.

| Directory | Purpose |
|-----------|---------|
| `./assets` | All game assets (images, sound effects, etc.) |
| `./config` | Shipped host JSON settings (e.g. `core.json`, `extensions.json`, scenarios) |
| `./entities` | All scenes for game elements within a root scene |
| `./extensions` | Built extension DLLs and copied per-extension content dirs for local load (see [extensions](features/platform/extensions.md)) |
| `./scenes` | All root scenes |
| `./src` | Source code (`Minimap.Simulation.Types`, `Minimap.Simulation`, `Minimap.Simulation.Navigation`, `Minimap.Extensive`, `Minimap.Client`, `Minimap.App`, `CompuQuest.Minimap`, …) |
| `./tests` | Test suite |
| `./ui` | All user interface scenes and related resources |
