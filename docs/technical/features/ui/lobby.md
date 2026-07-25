# Player lobby (technical)

Implements [lobby.md](../../../game/features/ui/lobby.md). Related: [local-input.md](../session/local-input.md), [local-play-context.md](../session/local-play-context.md), [user-profiles.md](../session/user-profiles.md), [players.md](../session/players.md), [accessories.md](../gameplay/accessories.md), [tags.md](../gameplay/tags.md).

## Requirements

- **Scene**: `res://scenes/lobby.tscn` — root `LobbyApp` (`Minimap.Client.Lobby`), four `LobbyPanel` instances (`Minimap.Client.Lobby`) in an `HBoxContainer`.
- **State**: pure `LobbyStateMachine` + per-slot `LobbyProfileSelectionState` + `LobbyAccessorySelectionState` in `Minimap.Client.Lobby` (unit-tested); Godot UI reflects slot modes, profile carousel, and accessory picks.
- **Modes**: `Available` → `SelectingProfile` → `SelectingAccessories` → `Ready`.
- **Profile UI**: `ProfileSelectionPanel` under each panel’s `CustomizeArea` while **SelectingProfile**. Select-only carousel (name + deaths); no create/rename/delete. Options omit profiles already confirmed by other slots. Empty/unavailable copy points at the main menu Profiles screen.
- **Accessory UI**: `AccessorySelectionPanel` FullRect under each panel’s `CustomizeArea` (expanding `Control`, not a whole-area scroller / clipper). Visible/interactive only in **SelectingAccessories**. Shell: points label + three named equal-expand children — `Available`, `Description`, `Owned` — each with a vertical-only `ScrollContainer` body. `Relayout(CustomizeArea.Size)` sets grid columns/icon size from width. Choices persist SelectingAccessories↔Ready (and when backing to SelectingProfile); cleared on Available. State model (`LobbyAccessorySelectionState`) tracks **prior-owned** (locked, cannot return) and **stage-owned** (can take/return). Available list is catalog minus all owned. Layout must keep selection inside the panel/viewport without horizontal overflow; a Godot playbook asserts the three children, vertical scroll regions, and width fit after advancing past profile selection.
- **Boot**: ordered via `LobbySceneBoot` — (1) bind panels and refresh UI, (2) load extensions + core accessory points + player profiles via `WorldHostHooks` (App registers loaders), (3) only then accept input. Unit tests cover each step and abort.
- **Boot failure**: on any exception during ready, abort the boot (no input, no panel refresh, no start-game), `GD.PushError`, and quit the process. Do **not** leave a half-initialized lobby interactive (Godot may still deliver input after a thrown `_Ready`).
- **Navigation**: when start gate passes, write [`LocalPlayContext`](../session/local-play-context.md) roster (devices + profile id/name + selected accessories) and `ChangeSceneToFile("res://scenes/world.tscn")`.
- **Entry**: reached from the [main menu](main-menu.md) (**New**) or `START_SCREEN=lobby`; `GodotRpcHost` default load path stays `world.tscn` for automation. `run/main_scene` is the main menu.
- **Automation**: `LobbyApp` implements `ILobbySnapshotSource` for playbook snapshots.

## Non-goals (for now)

- Character class/skin selection beyond accessories
- Profile CRUD in the lobby
