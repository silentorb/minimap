# Player lobby (technical)

Implements [../../game/features/lobby.md](../../game/features/lobby.md). Related: [local-input.md](local-input.md), [local-play-context.md](local-play-context.md).

## Requirements

- **Scene**: `res://scenes/lobby.tscn` — root `LobbyApp` (`Minimap.App`), four `LobbyPanel` instances (`Minimap.Client`) in an `HBoxContainer`.
- **State**: pure `LobbyStateMachine` in `Minimap.App` (unit-tested); Godot UI reflects slot modes.
- **Navigation**: when start gate passes, write [`LocalPlayContext`](local-play-context.md) roster and `ChangeSceneToFile("res://scenes/world.tscn")`.
- **Entry**: `project.godot` `run/main_scene` = lobby; `GodotRpcHost` default load path stays `world.tscn` for automation.
- **Automation**: `LobbyApp` implements `ILobbySnapshotSource` for playbook snapshots.

## Non-goals (for now)

- Character customization data model in lobby panels
