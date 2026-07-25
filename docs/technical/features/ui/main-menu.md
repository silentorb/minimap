# Main menu (technical)

Implements [../../../game/features/ui/main-menu.md](../../../game/features/ui/main-menu.md). Related: [lobby.md](lobby.md), [../session/local-input.md](../session/local-input.md), [../session/local-play-context.md](../session/local-play-context.md), [../platform/dotenv.md](../platform/dotenv.md).

## Requirements

- **Screen scene**: `res://scenes/main_menu.tscn` — root `MainMenuApp` under `Minimap.Client.MainMenu`. Full-rect black background, title label `CompuQuest Mini`, **New** / **Profiles** / **Quit** controls.
- **Entry**: `project.godot` `run/main_scene` = main menu. `GodotRpcHost` default load path stays `world.tscn` for automation.
- **`START_SCREEN`**: after dotenv, App exposes the env value. On main-menu ready, if trimmed value equals `lobby` (case-sensitive), `ChangeSceneToFile("res://scenes/lobby.tscn")`. Unset or any other value stays on the main menu screen.
- **New** (screen or popup) → `lobby.tscn` (lobby clears play context on enter).
- **Profiles** (screen only) → `profiles.tscn` ([user-profiles.md](../session/user-profiles.md)).
- **Quit** → process quit.
- **Popup scene**: `res://ui/main_menu_popup.tscn` — `CanvasLayer` with `process_mode = Always`, semi-transparent black overlay (alpha **0.6**), centered menu with Continue / New / Quit. Instanced under the world root.
- **Pause**: set `WorldApp` gameplay-paused flag (skip simulation tick); do **not** use `GetTree().Paused`.
- **Open**: in-world **Start** or **Escape** when not already in reconnect / game-over / main-menu modal. Escape still cancels placement/ability preview first when that cancel applies; otherwise Escape opens the menu.
- **Exclusive control**: store activating `InputDeviceId` (and owning player index); only that owner’s device may navigate/activate until dismiss.
- **Continue** / owner Start-or-Escape toggle dismisses popup and clears pause.
- Scene changes via `ChangeSceneToFile` must check `Error.Ok` (push error + throw on failure).

## Non-goals (for now)

- Pre-title splash scene routing beyond `START_SCREEN=lobby`
- InputMap action layer for menu binds
