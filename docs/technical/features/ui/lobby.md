# Player lobby (technical)

Implements [lobby.md](../../../game/features/ui/lobby.md). Related: [local-input.md](../session/local-input.md), [local-play-context.md](../session/local-play-context.md), [user-profiles.md](../session/user-profiles.md), [players.md](../session/players.md), [accessories.md](../gameplay/accessories.md), [tags.md](../gameplay/tags.md).

## Requirements

- **Scene**: `res://scenes/lobby.tscn` — root `LobbyApp` (`Minimap.Client.Lobby`), four `LobbyPanel` instances (`Minimap.Client.Lobby`) in an `HBoxContainer`.
- **State**: pure `LobbyStateMachine` + per-slot `LobbyProfileSelectionState` + `LobbyAccessorySelectionState` in `Minimap.Client.Lobby` (unit-tested); Godot UI reflects slot modes, profile carousel, and accessory picks.
- **Modes**: `Available` → `SelectingProfile` → `SelectingAccessories` → `Ready`.
- **Step nav UI**: each `LobbyPanel` has a bottom `NavRow` with `BackButton` / `ForwardButton`. Visibility follows `LobbyStepNavigation` (Back when not Available; Forward on SelectingProfile / SelectingAccessories). Presses invoke the same state-machine transitions as device Back / confirm-ready for that slot’s bound device. Forward is disabled when SelectingProfile has no available profiles. Buttons use `focus_mode` none so they do not steal pad/keyboard focus from accessory grids.
- **Profile UI**: `ProfileSelectionPanel` under each panel’s `CustomizeArea` while **SelectingProfile**. Select-only carousel (name + deaths + optional avatar preview); no create/rename/delete or avatar pick. Options omit profiles already confirmed by other slots. Empty/unavailable copy points at the main menu Profiles screen. After confirm, `LobbyPanel` shows a small avatar beside the profile display name in the slot title when present.
- **Roster**: start-game write includes `AvatarFile` (relative filename) with profile id/name so world HUD can resolve `user://profile_avatars/`.
- **Accessory UI**: `AccessorySelectionPanel` FullRect under each panel’s `CustomizeArea` (expanding `Control`, not a whole-area scroller / clipper). Visible/interactive only in **SelectingAccessories**. Shell: points label + three named equal-expand children — `Available`, `Description`, `Owned` — each with a vertical-only `ScrollContainer` body. `Relayout(CustomizeArea.Size)` sets grid columns/icon size from width. Choices persist SelectingAccessories↔Ready (and when backing to SelectingProfile); cleared on Available. State model (`LobbyAccessorySelectionState`) tracks **prior-owned** (locked, cannot return) and **stage-owned** (can take/return). Available list is catalog minus all owned. Layout must keep selection inside the panel/viewport without horizontal overflow; a Godot playbook asserts the three children, vertical scroll regions, and width fit after advancing past profile selection.
- **Boot**: ordered via `LobbySceneBoot` — (1) bind panels and refresh UI, (2) load extensions + core accessory points + player profiles via `WorldHostHooks` (App registers loaders), (3) only then accept input. Unit tests cover each step and abort.
- **Boot failure**: on any exception during ready, abort the boot (no input, no panel refresh, no start-game), `GD.PushError`, and quit the process. Do **not** leave a half-initialized lobby interactive (Godot may still deliver input after a thrown `_Ready`).
- **Navigation**: when start gate passes, write [`LocalPlayContext`](../session/local-play-context.md) roster (devices + profile id/name + selected accessories) and `ChangeSceneToFile("res://scenes/world.tscn")`.
- **Entry**: reached from the [main menu](main-menu.md) (**New**), `START_SCREEN=lobby`, or [post-session](post-session.md) → lobby. `GodotRpcHost` default load path stays `world.tscn` for automation. `run/main_scene` is the main menu.
- **Fresh vs restore**: main menu **New** / `START_SCREEN=lobby` → `Clear()` then empty machine. Returning-from-session → `LobbyStateMachine.ApplyFromRoster` (or equivalent): for each still-connected roster player, claim slot, restore confirmed profile + accessories, land in **SelectingAccessories** (not Ready). Skip disconnected joypads. Then clear the returning flag.
- **Leave to main menu**: primary player (lowest claimed slot index; if none, unbound Back/Escape) → `main_menu.tscn` and `Clear()` play context.
- **Automation**: `LobbyApp` implements `ILobbySnapshotSource` for playbook snapshots.

## Non-goals (for now)

- Character class/skin selection beyond accessories
- Profile CRUD in the lobby
