# Player lobby

Local multiplayer join screen before a match. Related: [local-input.md](local-input.md), [player-hud.md](player-hud.md).

## Requirements

- **Local multiplayer only** — no networking or remote join.
- The lobby is the **normal** way to start a game (`run/main_scene` points here).
- The screen is divided into **four horizontal panels**, one per possible player (1–4).
- Each panel has three modes in sequence:
  1. **Available** (inactive) — default; no player assigned.
  2. **Claimed** (active) — a device has joined this slot.
  3. **Ready** — that player is ready to play.
- Panels reserve **vertical space** for future character customization; content is mostly empty for now.
- **Claim**: an unassigned input device presses **activate** (`JoyButton.A` or `JoyButton.Start`, or keyboard Enter/Space) → claims the **lowest-index Available** panel and binds that device to the slot.
- **Ready**: while **Claimed**, the bound device presses **Start** (or keyboard Enter/Space) → **Ready**.
- **Back**: bound device presses **Back** (`JoyButton.B`, or keyboard Escape) → steps backward (Ready → Claimed → Available). Leaving Claimed releases devices for that slot.
- **Start game**: when **at least one** panel is claimed and **every claimed** panel is Ready (Available panels ignored) → navigate to the world scene with that player count and device bindings.
- A device already bound to a slot cannot claim another (except the reconnect flow in [local-input.md](local-input.md)).
- **Keyboard** may claim **one** lobby slot like a gamepad.

## Non-goals (for now)

- Character customization UI
- Online or remote lobby
- Changing panel count at runtime

## Developer / test entry

Loading `res://scenes/world.tscn` directly (without lobby) still starts a game for developers and automated tests; see [local-input.md](local-input.md).
