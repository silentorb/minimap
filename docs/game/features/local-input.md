# Local input

Input devices for local human players. Related: [lobby.md](lobby.md), [player-hud.md](player-hud.md), [combat.md](combat.md).

## Requirements

- Target **universal gamepad** support using Godot’s joypad names (`JoyButton.A`, `JoyButton.B`, `JoyButton.Start`, left stick, D-pad) — not vendor-specific labels.
- **Keyboard** may be used instead of a gamepad for one player.
- Each local player may bind **multiple devices** under the hood (one-to-many: one player aggregates input from every device in their set). The lobby UI only assigns the activating device on claim; there is no UI yet to attach extra devices (e.g. foot switches mapped to keyboard).
- **Lobby activate** (claim slot, reconnect): `JoyButton.A` or `JoyButton.Start`; keyboard **Enter** or **Space**.
- **Lobby ready**: `JoyButton.Start` while Claimed; keyboard Enter/Space.
- **Lobby back**: `JoyButton.B`; keyboard **Escape**.
- **In-world movement**: merge move axes from all devices bound to that player (arrow keys; left stick + D-pad on joypads).
- **Direct world start** (no lobby): default **solo keyboard** on player 1 for developers and automation.

### Disconnect / reconnect

- If a bound joypad **disconnects** during gameplay: **pause** the game and show a popup asking the player to reconnect.
- The player presses **activate** (A/Start) on a joypad **not assigned to any other player** — may be the same pad re-plugged or a replacement.
- If **other** local players still have connected devices, the popup includes **Drop player**; any other connected player may confirm with **activate** to remove the disconnected player and resume (quiet removal, same as death).
- If the dropped player was the last human, return to the lobby.

## Non-goals (for now)

- UI to attach multiple devices per player in the lobby
- Networked or remote input
- Per-player fire buttons (autoshoot remains shared)

## Xbox crosswalk (informative)

On common Xbox layouts: A ≈ `JoyButton.A`, B ≈ `JoyButton.B`, Menu ≈ `JoyButton.Start`. Implementation uses Godot universal ids only.
