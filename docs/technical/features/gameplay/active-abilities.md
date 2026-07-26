# Active abilities (technical)

Ability loadout and activation wiring. Implements [active-abilities.md](../../../game/features/gameplay/active-abilities.md). Related: [cell-placement.md](cell-placement.md), [interaction.md](interaction.md), [accessories.md](accessories.md), [controllers.md](controllers.md), [local-input.md](../session/local-input.md), [definition-config.md](../platform/definition-config.md), [hunger.md](hunger.md).

## Requirements

- **`AccessoryActivation`** / **`AccessoryActivationKind`** on **`AccessoryDefinition`** (JSON `activation.kind` / `activation.bind`).
- **`AbilityLoadout`** on **`Character`**: dedicated bind map + unbounded modal list; rebuilt on accessory add/remove and when enable state flips. Rebuild **skips disabled** accessories; preserves selection by accessory id when still present; if the previous selection dropped, selects the **next** remaining modal (then wrap/clamp to first if none after). **`CycleModal(delta)`** wraps selection through the pool; no-op when empty.
- **`PlayerController`**: feeds primary/secondary fire held, modal cycle (±1), ability activate/back edges, environment interact; owns preview state for modal abilities that use two-step activate (e.g. `ICellPlacementEffect`); for modals with **`IInstantUseEffect`** (no placement), activates immediately (pay cost + apply).
- **`Shoot.Tick(..., wantsFire)`**: players fire only when primary-fire dedicated accessory is present and fire is held; AI passes wantsFire when aim direction is non-zero.
- **`Swing.Tick(..., wantsSwing)`**: players swing only when secondary-fire dedicated accessory is present and secondary fire is held; AI swings when nearest hostile is within swing radius.

- Input: see game [local-input.md](../../../game/features/session/local-input.md).

## Non-goals (for now)

- Server-authoritative input buffering beyond per-tick edges
