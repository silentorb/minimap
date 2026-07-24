# Active abilities

In-world activation of ability accessories. Related: [accessories.md](accessories.md), [cell-placement.md](cell-placement.md), [farming.md](farming.md), [interaction.md](interaction.md), [combat.md](combat.md), [local-input.md](local-input.md). Technical: [../../technical/features/active-abilities.md](../../technical/features/active-abilities.md).

## Requirements

- Accessories declare an **activation** kind:
  - **Dedicated** — fixed to a named bind (Gun → `primary_fire`).
  - **Modal** — joins a switchable pool (up to **4**); player selects which is active, then activates it.
  - **None** — not player-activatable (e.g. grow on vegetables).
- Modal selection: gamepad **D-pad** (up/right/down/left → slots 1–4) or keyboard **1–4**.
- Modal activate: gamepad **`JoyButton.X`** / keyboard **Space**. Some abilities activate immediately; others use a two-step **preview → confirm** flow (e.g. cell placement). This is general, not plant- or placement-only.
- Cancel placement preview: gamepad **`JoyButton.B`** / keyboard **Escape**.
- Environment interact (separate from modal activate): gamepad **`JoyButton.A`** / keyboard **E** — see [interaction.md](interaction.md).
- Gun remains on dedicated primary fire (see [combat.md](combat.md)); it is not selected via the modal pool. Players only have Gun if they chose it in the lobby.

## Non-goals (for now)

- Ability cooldown UI / HUD slot chrome beyond selection
- Multiple modal pools or more than four modal slots
- AI using modal abilities (players first)
