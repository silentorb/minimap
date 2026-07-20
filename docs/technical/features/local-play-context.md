# Local play context

Cross-scene local multiplayer roster. Related: [lobby.md](lobby.md), [local-input.md](local-input.md).

## Requirements

- Godot autoload **`LocalPlayContextNode`** (`res://src/Minimap.Client/LocalPlayContextNode.cs`).
- Persists **player count** and **device sets** from lobby → world via `LocalPlayRoster` (`Minimap.App`).
- **`Clear()`** when lobby scene loads (fresh join session).
- **`ApplyDefaultSoloKeyboard()`** when world loads with empty roster (direct `world.tscn` / automation default).
- **`GameApp`** reads roster on `_Ready` to set `HumanPlayerCount` and wire `LocalInputAggregator`.

## Non-goals (for now)

- Saving roster to disk between app launches
