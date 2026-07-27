# Farming (technical)

Implements [farming.md](../../../game/features/gameplay/farming.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [interaction.md](interaction.md), [cell-placement.md](cell-placement.md), [resources.md](resources.md), [definition-config.md](../platform/definition-config.md), [hunger.md](hunger.md), [controllers.md](controllers.md).

## Requirements

- CompuQuest accessory **`farm`**: modal; effects include `modify_resource` (seeds +3 on acquire), `place_random_actor` (weighted vegetable pool + seed cost), and `harvest` (`IInteractionEffect`).
- Vegetable **growing** actor defs (`carrot_growing` / `corn_growing` / `melon_growing` / `crazed_carrot_growing`) under `config/actors/`: seedling depiction; accessory `grow_*` with `grow` effect (`durationSeconds`, `matureDepiction`, optional `harvestYield`, optional `emergeActorId` + `emergeAfterMatureSeconds`).
- Farm pool weights **3 / 3 / 3 / 1** for those growing defs.
- **`grow`** effect ticks on actors (`Tick(GameWorld, Actor, dt)`); on completion sets harvestable (**ripe**) and `DepictionOverride` to mature art. Ambush plants call `TryEmerge` after `emergeAfterMatureSeconds` post-ripe (or immediately on Farm harvest).
- **`harvest`**: ripe grow with no emerge id → remove actor + grant yield; ripe grow with `emergeActorId` → `TryEmerge` (no yield). Uses `GameWorld.SpawnChaseActor` (aggression **0.9** `AiController`) + `RivalFactionId`.
- **Picked** actor **`carrot_picked`**: accessory with `pickup_resource` (`IDefaultInteractionEffect` + energy use cost) → food ×1.
- Mobile actor **`crazed_carrot`**: accessories include `move`, `swing`, and `death_drop` → `carrot_picked`.
- Resource type **`food`** registered in CompuQuest; visible on HUD. Food is spent by **Eat** (see [hunger.md](hunger.md)).

## Non-goals (for now)

- Shared grow accessory id across vegetables without per-def yield/depiction
