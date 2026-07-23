# Player lobby (technical)

Implements [../../game/features/lobby.md](../../game/features/lobby.md). Related: [local-input.md](local-input.md), [local-play-context.md](local-play-context.md), [players.md](players.md), [accessories.md](accessories.md), [tags.md](tags.md).

## Requirements

- **Scene**: `res://scenes/lobby.tscn` — root `LobbyApp` (`Minimap.Client.Lobby`), four `LobbyPanel` instances (`Minimap.Client.Lobby`) in an `HBoxContainer`.
- **State**: pure `LobbyStateMachine` + per-slot `LobbyAccessorySelectionState` in `Minimap.Client.Lobby` (unit-tested); Godot UI reflects slot modes and accessory picks.
- **Accessory UI**: `AccessorySelectionPanel` under each panel’s `CustomizeArea` (`ScrollContainer`, clipped); selection is a normal scroll child (not FullRect-anchored into a bare `Control`). Visible/interactive only in **Claimed**. Choices persist Claimed↔Ready; cleared on Available. State model (`LobbyAccessorySelectionState`) tracks **prior-owned** (locked, cannot return) and **stage-owned** (can take/return). Available list is catalog minus all owned. Panel shows remaining points available. Layout must keep selection (and the claimed panel) inside the lobby viewport; a Godot playbook asserts global-rect containment after claim.
- **Boot**: ordered via `LobbySceneBoot` — (1) bind panels and refresh UI, (2) load extensions + core accessory points via `WorldHostHooks` (App registers loaders), (3) only then accept input. Unit tests cover each step and abort.
- **Boot failure**: on any exception during ready, abort the boot (no input, no panel refresh, no start-game), `GD.PushError`, and quit the process. Do **not** leave a half-initialized lobby interactive (Godot may still deliver input after a thrown `_Ready`).
- **Navigation**: when start gate passes, write [`LocalPlayContext`](local-play-context.md) roster (devices + selected accessories) and `ChangeSceneToFile("res://scenes/world.tscn")`.
- **Entry**: `project.godot` `run/main_scene` = lobby; `GodotRpcHost` default load path stays `world.tscn` for automation.
- **Automation**: `LobbyApp` implements `ILobbySnapshotSource` for playbook snapshots.

## Non-goals (for now)

- Character class/skin selection beyond accessories
