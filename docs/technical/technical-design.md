# Minimap technical design

# High-level requirements

- Godot is the game engine
- C# is the primary programming language
- Mostly developed by AI agents
- Heavily requirements-driven, documented under the `./docs` directory. **`./docs` is the source of truth for functionality**: game rules and feel live under `docs/game/`; architecture and contracts live under `docs/technical/`. Code and tests implement those documents. When behavior changes, update the docs in the same change (or first). If code and docs disagree, docs win and code is fixed.
- Heavily test-driven, using both unit tests and functional tests. Tests verify **documented** requirements (values and rules stated in feature docs), not undocumented code quirks. User-reported gaps that the suite missed get a regression test when a sound one exists at the lowest practical layer; otherwise escalate rather than adding brittle or flaky coverage (see [features/testing.md](features/testing.md) **Bug regressions / debugging**).
- Prefer **explicit error outcomes** for expected failures; use **exceptions** only for truly exceptional cases or documented fail-fast abort boundaries (see [features/error-handling.md](features/error-handling.md)).
- No global state, except where needed for integration with Godot and third-party libraries
- Clean separation between visual game state and simulation game state
- **C# project boundaries**:
  - **Minimap.Simulation.Types** — shared definition/content types (little/no logic); depended on by Simulation and Extensive
  - **Minimap.Simulation** — authoritative logic/state; no Godot; no user input or output
  - **Minimap.Extensive** — extension contracts, registry, and default integrator (no Godot, no file I/O)
  - **Minimap.Client** — Godot rendering, input capture, HUD; may reference Simulation sparingly
  - **Minimap.App** — thin front-facing integration that wires Simulation and Client (session creation, attach player controllers, feed HUD models); loads extension assemblies from config
  - **CompuQuest.Minimap** — sample/content extension library (loadable DLL; build-only host dependency, not linked into the Godot assembly)

# Godot project layout

Below are the directories used to store Godot-related files.

It is not an exhaustive list of all the directories in this project.

| Directory | Purpose |
|-----------|---------|
| `./assets` | All game assets (images, sound effects, etc.) |
| `./config` | Shipped JSON settings (e.g. `core.json`, `extensions.json`) |
| `./entities` | All scenes for game elements within a root scene |
| `./extensions` | Built extension DLLs copied here for local load (see [extensions](features/extensions.md)) |
| `./scenes` | All root scenes |
| `./src` | Source code (`Minimap.Simulation.Types`, `Minimap.Simulation`, `Minimap.Extensive`, `Minimap.Client`, `Minimap.App`, `CompuQuest.Minimap`, …) |
| `./tests` | Test suite |
| `./ui` | All user interface scenes and related resources |
