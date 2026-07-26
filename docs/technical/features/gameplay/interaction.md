# Environment interaction (technical)

Implements [interaction.md](../../../game/features/gameplay/interaction.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [farming.md](farming.md), [local-input.md](../session/local-input.md), [controllers.md](controllers.md).

## Requirements

- Simulation contract **`IInteractionEffect`**: `CanInteract(world, actor, target)` / `TryInteract(...)`. Expected rejection returns `false`.
- Simulation contract **`IDefaultInteractionEffect`**: same shape as `IInteractionEffect`; lives on the **object** (target) actor’s effects. Used when no overriding ability interaction applies.
- Target resolution: actor occupying the cell from `CellFacing.CellInFront` (same facing/proximity model as placement).
- Validity / invoke (`EnvironmentInteraction`):
  1. Resolve front-cell target.
  2. Find **default** via first `IDefaultInteractionEffect` on the target with `CanInteract`.
  3. If the selected modal has an `IInteractionEffect` with `CanInteract`, that effect **overrides** the default.
  4. Highlight and invoke the chosen effect (or none).
- Client: rising-edge interact input; highlight the resolved target actor node; clear when invalid or ability changes.
- CompuQuest **`harvest`** implements `IInteractionEffect` (ability-side) for mature food / crazed crops; shipped use cost **1 energy**.
- CompuQuest **`use_computer`** implements `IInteractionEffect` for actors whose definition id is **`computer`**; shipped use cost **1 energy**.
- CompuQuest **`pickup_resource`** implements `IDefaultInteractionEffect` + **`IEffectUseCost`** on free-loot actors (e.g. loose carrot → food ×1; shipped use cost **1 energy**).

## Non-goals (for now)

- Continuous (non-hex) proximity radii
- Queued or channeled interact animations
