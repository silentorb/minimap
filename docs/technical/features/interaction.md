# Environment interaction (technical)

Implements [../../game/features/interaction.md](../../game/features/interaction.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [farming.md](farming.md), [local-input.md](local-input.md), [controllers.md](controllers.md).

## Requirements

- Simulation contract **`IInteractionEffect`**: `CanInteract(world, actor, target)` / `TryInteract(...)`. Expected rejection returns `false`.
- Target resolution: actor occupying the cell from `CellFacing.CellInFront` (same facing/proximity model as placement).
- Validity: scan interaction effects on the selected modal accessory (equipped ability). First matching effect that can interact wins for highlight/invoke.
- Client: rising-edge interact input; highlight the resolved target actor node; clear when invalid or ability changes.
- CompuQuest **`harvest`** implements `IInteractionEffect` for mature food actors.
- CompuQuest **`use_computer`** implements `IInteractionEffect` for actors whose definition id is **`computer`** (successful interact; further gameplay forthcoming).

## Non-goals (for now)

- Continuous (non-hex) proximity radii
- Queued or channeled interact animations
