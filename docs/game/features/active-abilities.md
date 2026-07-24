# Active abilities

In-world activation of accessories. Related: [accessories.md](accessories.md), [cell-placement.md](cell-placement.md), [combat.md](combat.md), [local-input.md](local-input.md). Technical: [../../technical/features/active-abilities.md](../../technical/features/active-abilities.md).

## Requirements

- Accessories declare an **activation** kind:
  - **Dedicated** — fixed to a named bind (Gun → `primary_fire`).
  - **Modal** — joins a switchable pool (up to **4**); player selects which is active, then activates it.
  - **None** — not player-activatable.
- Modal selection: gamepad **D-pad** (up/right/down/left → slots 1–4) or keyboard **1–4**.
- Modal activate: gamepad **`JoyButton.X`** / keyboard **Space**. Two presses for placement abilities (preview → confirm).
- Cancel placement preview: gamepad **`JoyButton.B`** / keyboard **Escape**.
- **`JoyButton.A`** is reserved for future environment interaction (not used for abilities in-world).
- Gun remains on dedicated primary fire (see [combat.md](combat.md)); it is not selected via the modal pool.

## Non-goals (for now)

- Ability cooldown UI / HUD slot chrome beyond selection
- Multiple modal pools or more than four modal slots
- AI using modal abilities (players first)
