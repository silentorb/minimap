# Player lobby

Local multiplayer join screen before a match. Related: [local-input.md](local-input.md), [player-hud.md](player-hud.md), [players.md](players.md), [accessories.md](accessories.md), [tags.md](tags.md).

## Requirements

- **Local multiplayer only** — no networking or remote join.
- The lobby is the **normal** way to start a game (`run/main_scene` points here).
- The screen is divided into **four horizontal panels**, one per possible player (1–4).
- Each panel has three modes in sequence:
  1. **Available** (inactive) — default; no player assigned.
  2. **Claimed** (active) — a device has joined this slot; **accessory selection panel** is shown and interactive.
  3. **Ready** — that player is ready to play; accessory panel is hidden but **chosen accessories and remaining points persist**.
- Accessory selection UI (Claimed only) fills the player panel’s customize area and scales to that parent (no clipping the main picker). A thin **points available** line sits above three equal-height child panels that share the remaining vertical space; each scrolls **vertically** when its content overflows:
  1. **Available** — icon grid of unowned selectable accessories.
  2. **Description** — name, cost, and body of the focused accessory.
  3. **Owned** — prior-stage locked accessories plus this-stage choices.
  Activating an available accessory spends points and moves it to Owned; activating a this-stage owned accessory returns it to Available and refunds points. Accessories acquired in a **previous stage** stay in Owned and cannot be unchosen. Icon grids scale to the panel width (no horizontal scroll). Budget comes from core **`player.accessoryPoints`** (default 2). Catalog comes from the integrator’s player-selectable accessories. The selection UI must remain **inside its player panel** and **inside the lobby viewport**.
- Input while Claimed: arrows / D-pad / stick navigate grids; **Space** / **A** take or return the focused accessory (if allowed); **Enter** / **Start** ready up; **Escape** / **B** back (Ready→Claimed keeps choices; Claimed→Available clears selection and devices).
- Players need not spend all accessory points to ready.
- **Claim**: an unassigned input device presses **activate** (`JoyButton.A` or `JoyButton.Start`, or keyboard Enter/Space) → claims the **lowest-index Available** panel and binds that device to the slot.
- **Start game**: when **at least one** panel is claimed and **every claimed** panel is Ready (Available panels ignored) → navigate to the world scene with that player count, device bindings, and selected accessories.
- A device already bound to a slot cannot claim another (except the reconnect flow in [local-input.md](local-input.md)).
- **Keyboard** may claim **one** lobby slot like a gamepad.

## Non-goals (for now)

- Online or remote lobby
- Changing panel count at runtime

## Developer / test entry

Loading `res://scenes/world.tscn` directly (without lobby) still starts a game for developers and automated tests; see [local-input.md](local-input.md). Direct world entry creates players with default accessory points and **no** selected accessories.
