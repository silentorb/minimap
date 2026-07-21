# Player lobby (technical)

Implements [../../game/features/lobby.md](../../game/features/lobby.md). Related: [local-input.md](local-input.md), [local-play-context.md](local-play-context.md).

## Requirements

- **Scene**: `res://scenes/lobby.tscn` — root `LobbyApp` (`Minimap.Client.Lobby`), four `LobbyPanel` instances (`Minimap.Client.Lobby`) in an `HBoxContainer`.
- **State**: pure `LobbyStateMachine` in `Minimap.Client.Lobby` (unit-tested); Godot UI reflects slot modes.
- **Boot**: ordered via `LobbySceneBoot` — (1) bind panels and refresh UI, (2) preflight-load `extensions.json` via `ExtensionPreflight` (App registers the real `ExtensionLoader`), (3) only then accept input. Unit tests cover each step and abort.
- **Boot failure**: on any exception during ready, abort the boot (no input, no panel refresh, no start-game), `GD.PushError`, and quit the process. Do **not** leave a half-initialized lobby interactive (Godot may still deliver input after a thrown `_Ready`).
- **Navigation**: when start gate passes, write [`LocalPlayContext`](local-play-context.md) roster and `ChangeSceneToFile("res://scenes/world.tscn")`.
- **Entry**: `project.godot` `run/main_scene` = lobby; `GodotRpcHost` default load path stays `world.tscn` for automation.
- **Automation**: `LobbyApp` implements `ILobbySnapshotSource` for playbook snapshots.

## Non-goals (for now)

- Character customization data model in lobby panels
