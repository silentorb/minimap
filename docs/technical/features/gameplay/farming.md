# Farming (technical)

Implements [farming.md](../../../game/features/gameplay/farming.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [interaction.md](interaction.md), [cell-placement.md](cell-placement.md), [resources.md](resources.md), [definition-config.md](../platform/definition-config.md), [hunger.md](hunger.md).

## Requirements

- CompuQuest accessory **`farm`**: modal; effects include `modify_resource` (seeds +3 on acquire), `place_random_actor` (weighted vegetable pool + seed cost), and `harvest` (`IInteractionEffect`).
- Vegetable **actor** defs (`carrot` / `corn` / `melon`) under `config/actors/`: seedling depiction; accessory `grow_*` with `grow` effect (`durationSeconds`, `matureDepiction`, `harvestYield`).
- **`grow`** effect ticks on cell actors; on completion sets harvestable and `DepictionOverride` to mature art.
- **`harvest`** validates mature grow state on the target; removes the cell actor; grants yield resource to the acting character.
- Resource type **`food`** registered in CompuQuest; visible on HUD. Food is spent by **Eat** (see [hunger.md](hunger.md)).

## Non-goals (for now)

- Shared grow accessory id across vegetables without per-def yield/depiction
