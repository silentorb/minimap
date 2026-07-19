# Minimap technical design

# High-level requirements

- Godot is the game engine
- C# is the primary programming language
- Mostly developed by AI agents
- Heavily requirements-driven, documented under the `./docs` directory. **`./docs` is the source of truth for functionality**: game rules and feel live under `docs/game/`; architecture and contracts live under `docs/technical/`. Code and tests implement those documents. When behavior changes, update the docs in the same change (or first). If code and docs disagree, docs win and code is fixed.
- Heavily test-driven, using both unit tests and functional tests. Tests verify **documented** requirements (values and rules stated in feature docs), not undocumented code quirks.
- No global state, except where needed for integration with Godot and third-party libraries
- Clean separation between visual game state and simulation game state
- **C# project boundaries**:
  - **Minimap.Simulation** — authoritative logic/state; no Godot; no user input or output
  - **Minimap.Client** — Godot rendering, input capture, HUD; may reference Simulation sparingly
  - **Minimap.App** — thin front-facing integration that wires Simulation and Client (session creation, attach player controllers, feed HUD models)

# Godot project layout

Below are the directories used to store Godot-related files.

It is not an exhaustive list of all the directories in this project.

| Directory | Purpose |
|-----------|---------|
| `./assets` | All game assets (images, sound effects, etc.) |
| `./config` | Shipped JSON settings (e.g. `core.json`) |
| `./entities` | All scenes for game elements within a root scene |
| `./scenes` | All root scenes |
| `./src` | Source code (`Minimap.Simulation`, `Minimap.Client`, `Minimap.App`) |
| `./tests` | Test suite |
| `./ui` | All user interface scenes and related resources |
