# Main menu (technical)

Implements [../../../game/features/ui/main-menu.md](../../../game/features/ui/main-menu.md). Related: [lobby.md](lobby.md), [../session/local-input.md](../session/local-input.md), [../session/local-play-context.md](../session/local-play-context.md), [../platform/dotenv.md](../platform/dotenv.md).

## Requirements

- **Screen scene**: `res://scenes/main_menu.tscn` — root `MainMenuApp` under `Minimap.Client.MainMenu`. Full-rect black background, title label `CompuQuest Mini`, **New** / **Profiles** / **Quit** controls.
- **Entry**: `project.godot` `run/main_scene` = main menu. `GodotRpcHost` default load path stays `world.tscn` for automation.
- **`START_SCREEN`**: after dotenv, App exposes the env value. On main-menu ready, if trimmed value equals `lobby` (case-sensitive), `ChangeSceneToFile("res://scenes/lobby.tscn")`. Unset or any other value stays on the main menu screen.
- **New** (screen only) → `lobby.tscn` (fresh lobby; clears play context on enter).
- **Profiles** (screen only) → `profiles.tscn` ([user-profiles.md](../session/user-profiles.md)).
- **Quit** → process quit.
- **Popup scene**: `res://ui/main_menu_popup.tscn` — `CanvasLayer` with `process_mode = Always`, semi-transparent black overlay (alpha **0.6**), centered menu with Continue / **End game** / Quit. Instanced under the world root. **End game** → [post-session](post-session.md) (not a scene change).
- **Pause**: set `WorldApp` gameplay-paused flag (skip simulation tick); do **not** use `GetTree().Paused`.
- **Open**: in-world **Start** or **Escape** when not already in reconnect / post-session / main-menu modal. Escape still cancels placement/ability preview first when that cancel applies; otherwise Escape opens the menu.
- **Exclusive control**: store activating `InputDeviceId` (and owning player index); only that owner’s device may navigate/activate until dismiss. Non-owner keys/joypads/mouse are ignored while open (`MainMenuPopupController` + `_Input` before GUI).
- **Screen navigation**: `MainMenuScreenController` + `MainMenuApp._Input` (before GUI) — D-pad Up/Down or keyboard Up/Down move selection over New / Profiles / Quit (wraps); **A** / Start / Enter / Space activate the selected option. Selection focus is mirrored with `GrabFocus`.
- **Owner navigation** (popup): D-pad Up/Down or keyboard Up/Down move selection (wraps); **A** / Enter / Space activate the selected option; Start / Escape dismiss as **Continue**. Selection focus is mirrored with `GrabFocus` on the active button.
- **Continue** / owner Start-or-Escape toggle dismisses popup and clears pause.
- World must **not** `ChangeSceneToFile` to `main_menu.tscn`; after a match starts, main menu is reached via lobby leave.
- Scene changes via `ChangeSceneToFile` must check `Error.Ok` (push error + throw on failure).

## Non-goals (for now)

- Pre-title splash scene routing beyond `START_SCREEN=lobby`
- InputMap action layer for menu binds
