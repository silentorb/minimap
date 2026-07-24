# Local input

Input devices for local human players. Related: [lobby.md](lobby.md), [movement.md](movement.md), [player-hud.md](player-hud.md), [combat.md](combat.md), [active-abilities.md](active-abilities.md).

## Requirements

- Target **universal gamepad** support using Godot’s joypad names (`JoyButton.A`, `JoyButton.B`, `JoyButton.X`, `JoyButton.Start`, left stick, right stick, D-pad) — not vendor-specific labels.
- **Keyboard** may be used instead of a gamepad for one player.
- Each local player may bind **multiple devices** under the hood (one-to-many: one player aggregates input from every device in their set). The lobby UI only assigns the activating device on claim; there is no UI yet to attach extra devices (e.g. foot switches mapped to keyboard).
- **Lobby activate** (claim slot, reconnect): `JoyButton.A` or `JoyButton.Start`; keyboard **Enter** or **Space**.
- **Lobby ready**: `JoyButton.Start` while Claimed; keyboard Enter/Space.
- **Lobby back**: `JoyButton.B`; keyboard **Escape**.
- **In-world movement**: **WASD**; joypad **left stick only** (D-pad does **not** move).
- **In-world aim**: joypad **right stick**; keyboard **mouse** (direction from pawn to cursor). Arrow keys do not aim.
- **In-world primary fire** (dedicated Gun): joypad **Right Trigger**; keyboard **Left mouse button**. Aim alone does not fire.
- **Modal ability select**: joypad **D-pad** (up/right/down/left → slots 1–4); keyboard **1–4**.
- **Modal ability activate**: joypad **`JoyButton.X`**; keyboard **Space**. (Immediate or preview/confirm depending on the ability; see [active-abilities.md](active-abilities.md).)
- **Environment interact**: joypad **`JoyButton.A`**; keyboard **E** (see [interaction.md](interaction.md)).
- **Cancel placement preview**: joypad **`JoyButton.B`**; keyboard **Escape**.
- **Direct world start** (no lobby): default **solo keyboard** on player 1 for developers and automation.

### Disconnect / reconnect

- If a bound joypad **disconnects** during gameplay: **pause** the game and show a popup asking the player to reconnect.
- The player presses **activate** (A/Start) on a joypad **not assigned to any other player** — may be the same pad re-plugged or a replacement.
- If **other** local players still have connected devices, the popup includes **Drop player**; any other connected player may confirm with **activate** to remove the disconnected player and resume (quiet removal, same as death).
- If the dropped player was the last human, return to the lobby.

## Non-goals (for now)

- UI to attach multiple devices per player in the lobby
- Networked or remote input

## Xbox crosswalk (informative)

On common Xbox layouts: A ≈ `JoyButton.A`, B ≈ `JoyButton.B`, X ≈ `JoyButton.X`, Menu ≈ `JoyButton.Start`. Implementation uses Godot universal ids only.
