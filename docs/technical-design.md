# Minimap technical design

# High-level requirements

- Godot is the game engine
- C# is the primary programming language
- Mostly developed by AI agents
- Heavily requirements-driven, documented under the `./docs` directory.
- Heavily test-driven, using both unit tests and functional tests
- No global state, except where needed for integration with Godot and third-party libraries
- Clean separation between visual game state and simulation game state

# Godot project layout

Below are the directories used to store Godot-related files.

It is not an exhaustive list of all the directories in this project.

| Directory | Purpose |
|-----------|---------|
| `./assets` | All game assets (images, sound effects, etc.) |
| `./entities` | All scenes for game elements within a root scene |
| `./scenes` | All root scenes |
| `./src` | Source code |
| `./tests` | Test suite |
| `./ui` | All user interface scenes and related resources |
