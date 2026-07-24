# Active abilities (technical)

Ability loadout and activation wiring. Implements [../../game/features/active-abilities.md](../../game/features/active-abilities.md). Related: [cell-placement.md](cell-placement.md), [interaction.md](interaction.md), [accessories.md](accessories.md), [controllers.md](controllers.md), [local-input.md](local-input.md), [definition-config.md](definition-config.md).

## Requirements

- **`AccessoryActivation`** / **`AccessoryActivationKind`** on **`AccessoryDefinition`** (JSON `activation.kind` / `activation.bind`).
- **`AbilityLoadout`** on **`Character`**: dedicated bind map + modal list (max 4); rebuilt on accessory add/remove.
- **`PlayerController`**: feeds fire held, modal select, ability activate/back edges, environment interact; owns preview state for modal abilities that use two-step activate (e.g. `ICellPlacementEffect`).
- **`Shoot.Tick(..., wantsFire)`**: players fire only when primary-fire dedicated accessory is present and fire is held; AI passes wantsFire when aim direction is non-zero.
- Input: see game [local-input.md](../../game/features/local-input.md).

## Non-goals (for now)

- Server-authoritative input buffering beyond per-tick edges
