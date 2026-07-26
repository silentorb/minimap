# Farming (technical)

Implements [farming.md](../../../game/features/gameplay/farming.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [interaction.md](interaction.md), [cell-placement.md](cell-placement.md), [resources.md](resources.md), [definition-config.md](../platform/definition-config.md), [hunger.md](hunger.md), [controllers.md](controllers.md).

## Requirements

- CompuQuest accessory **`farm`**: modal; effects include `modify_resource` (seeds +3 on acquire), `place_random_actor` (weighted vegetable pool + seed cost), and `harvest` (`IInteractionEffect`).
- Vegetable **actor** defs (`carrot` / `corn` / `melon` / `crazed_carrot`) under `config/actors/`: seedling depiction; accessory `grow_*` with `grow` effect (`durationSeconds`, `matureDepiction`, optional `harvestYield`, optional `emergeCharacterId` + `emergeAfterMatureSeconds`).
- Farm pool weights **3 / 3 / 3 / 1** for carrot / corn / melon / crazed_carrot.
- **`grow`** effect ticks on cell actors (`Tick(GameWorld, Actor, dt)`); on completion sets harvestable and `DepictionOverride` to mature art. Ambush crops call `TryEmerge` after `emergeAfterMatureSeconds` post-mature (or immediately on Farm harvest).
- **`harvest`**: mature grow with no emerge id → remove actor + grant yield; mature grow with `emergeCharacterId` → `TryEmerge` (no yield). Uses `GameWorld.SpawnChaseCharacter` (aggression **0.9** `AiController`) + `RivalFactionId`.
- Free-loot actor **`loose_carrot`**: accessory with `pickup_resource` (`IDefaultInteractionEffect` + energy use cost) → food ×1.
- Character **`crazed_carrot`**: accessories include `swing` and `death_drop` → `loose_carrot`.
- Resource type **`food`** registered in CompuQuest; visible on HUD. Food is spent by **Eat** (see [hunger.md](hunger.md)).

## Non-goals (for now)

- Shared grow accessory id across vegetables without per-def yield/depiction
