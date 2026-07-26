# Active abilities

In-world activation of ability accessories. Related: [accessories.md](accessories.md), [cell-placement.md](cell-placement.md), [farming.md](farming.md), [interaction.md](interaction.md), [combat.md](combat.md), [local-input.md](../session/local-input.md), [hunger.md](hunger.md), [player-hud.md](../ui/player-hud.md). Technical: [active-abilities.md](../../../technical/features/gameplay/active-abilities.md).

## Requirements

- Accessories declare an **activation** kind:
  - **Dedicated** — fixed to a named bind (Gun → `primary_fire`, Swing → `secondary_fire`).
  - **Modal** — joins an unbounded switchable pool; player cycles which is active, then activates it.
    - **None** — not player-activatable (e.g. grow on vegetables).
- Modal selection: gamepad **D-pad Left / Right** or keyboard **`[` / `]`** cycle previous / next (wraps). The cycle includes an explicit **none** (unequipped) entry so the player can have no modal ability selected.
- Modal activate: gamepad **`JoyButton.X`** / keyboard **Space**. Some abilities activate immediately (**instant use**, e.g. Eat); others use a two-step **preview → confirm** flow (e.g. cell placement). This is general, not plant- or placement-only. Activate is a no-op when **none** is selected.
- Cancel placement preview: gamepad **`JoyButton.B`** / keyboard **Escape**.
- Environment interact (separate from modal activate): gamepad **`JoyButton.A`** / keyboard **E** — see [interaction.md](interaction.md). Interact is object-relative and can use defaults when **none** is equipped.
- Gun remains on dedicated primary fire and Swing on dedicated secondary fire (see [combat.md](combat.md)); they are not selected via the modal pool. Players only have them if chosen in the lobby (or granted by a character definition).

- **Disabled** accessories are excluded from the modal loadout (and HUD). When the selected modal becomes disabled or is removed, selection remaps to another remaining modal (or stays on **none** if that was selected). Inability to pay a use cost does **not** exclude an ability.
- The HUD shows the currently selected modal ability (icon + name); see [player-hud.md](../ui/player-hud.md). Hidden when **none** is selected.


## Non-goals (for now)

- Ability cooldown UI beyond selected-ability chrome
- Multiple modal pools
- AI using modal abilities (players first)
