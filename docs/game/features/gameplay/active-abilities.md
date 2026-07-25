# Active abilities

In-world activation of ability accessories. Related: [accessories.md](accessories.md), [cell-placement.md](cell-placement.md), [farming.md](farming.md), [interaction.md](interaction.md), [combat.md](combat.md), [local-input.md](../session/local-input.md), [hunger.md](hunger.md), [player-hud.md](../ui/player-hud.md). Technical: [active-abilities.md](../../../technical/features/gameplay/active-abilities.md).

## Requirements

- Accessories declare an **activation** kind:
  - **Dedicated** — fixed to a named bind (Gun → `primary_fire`).
  - **Modal** — joins a switchable pool (up to **4**); player selects which is active, then activates it.
  - **None** — not player-activatable (e.g. grow on vegetables).
- Modal selection: gamepad **D-pad** (up/right/down/left → slots 1–4) or keyboard **1–4**.
- Modal activate: gamepad **`JoyButton.X`** / keyboard **Space**. Some abilities activate immediately (**instant use**, e.g. Eat); others use a two-step **preview → confirm** flow (e.g. cell placement). This is general, not plant- or placement-only.
- Cancel placement preview: gamepad **`JoyButton.B`** / keyboard **Escape**.
- Environment interact (separate from modal activate): gamepad **`JoyButton.A`** / keyboard **E** — see [interaction.md](interaction.md).
- Gun remains on dedicated primary fire (see [combat.md](combat.md)); it is not selected via the modal pool. Players only have Gun if they chose it in the lobby.
- **Disabled** accessories are excluded from the modal loadout (and HUD). When the selected modal becomes disabled or is removed, selection remaps to another remaining modal. Inability to pay a use cost does **not** exclude an ability.
- The HUD shows the currently selected modal ability (icon + name); see [player-hud.md](../ui/player-hud.md).

## Non-goals (for now)

- Ability cooldown UI beyond selected-ability chrome
- Multiple modal pools or more than four modal slots
- AI using modal abilities (players first)
