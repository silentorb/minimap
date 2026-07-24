# Active abilities (technical)

Ability loadout and activation wiring. Implements [../../game/features/active-abilities.md](../../game/features/active-abilities.md). Related: [cell-placement.md](cell-placement.md), [interaction.md](interaction.md), [accessories.md](accessories.md), [controllers.md](controllers.md), [local-input.md](local-input.md), [definition-config.md](definition-config.md), [hunger.md](hunger.md).

## Requirements

- **`AccessoryActivation`** / **`AccessoryActivationKind`** on **`AccessoryDefinition`** (JSON `activation.kind` / `activation.bind`).
- **`AbilityLoadout`** on **`Character`**: dedicated bind map + modal list (max 4); rebuilt on accessory add/remove and when enable state flips. Rebuild **skips disabled** accessories; preserves selection by accessory id when still present; if the previous selection dropped, selects the **next** remaining modal (then wrap/clamp to first if none after).
- **`PlayerController`**: feeds fire held, modal select, ability activate/back edges, environment interact; owns preview state for modal abilities that use two-step activate (e.g. `ICellPlacementEffect`); for modals with **`IInstantUseEffect`** (no placement), activates immediately (pay cost + apply).
- **`Shoot.Tick(..., wantsFire)`**: players fire only when primary-fire dedicated accessory is present and fire is held; AI passes wantsFire when aim direction is non-zero.
- Input: see game [local-input.md](../../game/features/local-input.md).

## Non-goals (for now)

- Server-authoritative input buffering beyond per-tick edges
