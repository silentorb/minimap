# Player lobby (technical)

Implements [../../game/features/lobby.md](../../game/features/lobby.md). Related: [local-input.md](local-input.md), [local-play-context.md](local-play-context.md), [players.md](players.md), [accessories.md](accessories.md), [tags.md](tags.md).

## Requirements

- **Scene**: `res://scenes/lobby.tscn` — root `LobbyApp` (`Minimap.Client.Lobby`), four `LobbyPanel` instances (`Minimap.Client.Lobby`) in an `HBoxContainer`.
- **State**: pure `LobbyStateMachine` + per-slot `LobbyAccessorySelectionState` in `Minimap.Client.Lobby` (unit-tested); Godot UI reflects slot modes and accessory picks.
- **Accessory UI**: `AccessorySelectionPanel` FullRect under each panel’s `CustomizeArea` (expanding `Control`, not a whole-area scroller / clipper). Visible/interactive only in **Claimed**. Shell: points label + three named equal-expand children — `Available`, `Description`, `Owned` — each with a vertical-only `ScrollContainer` body. `Relayout(CustomizeArea.Size)` sets grid columns/icon size from width. Choices persist Claimed↔Ready; cleared on Available. State model (`LobbyAccessorySelectionState`) tracks **prior-owned** (locked, cannot return) and **stage-owned** (can take/return). Available list is catalog minus all owned. Layout must keep selection inside the panel/viewport without horizontal overflow; a Godot playbook asserts the three children, vertical scroll regions, and width fit after claim.
- **Boot**: ordered via `LobbySceneBoot` — (1) bind panels and refresh UI, (2) load extensions + core accessory points via `WorldHostHooks` (App registers loaders), (3) only then accept input. Unit tests cover each step and abort.
- **Boot failure**: on any exception during ready, abort the boot (no input, no panel refresh, no start-game), `GD.PushError`, and quit the process. Do **not** leave a half-initialized lobby interactive (Godot may still deliver input after a thrown `_Ready`).
- **Navigation**: when start gate passes, write [`LocalPlayContext`](local-play-context.md) roster (devices + selected accessories) and `ChangeSceneToFile("res://scenes/world.tscn")`.
- **Entry**: `project.godot` `run/main_scene` = lobby; `GodotRpcHost` default load path stays `world.tscn` for automation.
- **Automation**: `LobbyApp` implements `ILobbySnapshotSource` for playbook snapshots.

## Non-goals (for now)

- Character class/skin selection beyond accessories
