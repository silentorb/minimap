# Local play context

Cross-scene local multiplayer roster. Related: [lobby.md](../ui/lobby.md), [local-input.md](local-input.md).

## Requirements

- Godot autoload **`LocalPlayContextNode`** (`res://src/Minimap.Client/LocalPlayContextNode.cs`).
- Persists **player count**, **device sets**, **selected user profile** (`ProfileId` / `DisplayName` when present), and **selected accessories** from lobby → world via `LocalPlayRoster` (`Minimap.Client.LocalPlay`). `ApplyFromLobby` uses `LocalPlayRoster.CopyFrom` so profile and accessory picks are not dropped.
- **`Clear()`** on fresh lobby entry from the main menu (or when leaving lobby to the main menu).
- **Return from session**: world sets a returning-from-session marker and leaves the roster intact; lobby hydrates from the roster instead of clearing (see [lobby.md](../ui/lobby.md)).
- **`ProfilesFocusId`**: optional `Guid` set by Profiles before opening the Achievements scene; cleared when leaving Achievements / Profiles as appropriate.
- **`ApplyDefaultSoloKeyboard()`** when world loads with empty roster (direct `world.tscn` / automation default).
- **`WorldApp`** reads roster on `_Ready` to set human player count, spawn selected accessories onto pawns, and wire `LocalInputAggregator`.

## Non-goals (for now)

- Saving roster to disk between app launches
