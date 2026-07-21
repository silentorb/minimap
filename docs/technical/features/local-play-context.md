# Local play context

Cross-scene local multiplayer roster. Related: [lobby.md](lobby.md), [local-input.md](local-input.md).

## Requirements

- Godot autoload **`LocalPlayContextNode`** (`res://src/Minimap.Client/LocalPlayContextNode.cs`).
- Persists **player count** and **device sets** from lobby → world via `LocalPlayRoster` (`Minimap.Client.LocalPlay`).
- **`Clear()`** when lobby scene loads (fresh join session).
- **`ApplyDefaultSoloKeyboard()`** when world loads with empty roster (direct `world.tscn` / automation default).
- **`WorldApp`** reads roster on `_Ready` to set human player count and wire `LocalInputAggregator`.

## Non-goals (for now)

- Saving roster to disk between app launches
