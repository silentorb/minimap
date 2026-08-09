# Actors and factions

Simulation actor model and faction APIs. Implements [factions.md](../../../game/features/gameplay/factions.md) and [health.md](../../../game/features/gameplay/health.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [resources.md](resources.md), [controllers.md](controllers.md).

## Requirements

- **`Actor`** carries **`FactionId`** (`int`, default **0**). Placeables receive it from `TryPlaceActor` (Geek placement passes the placer’s faction). Free spawns set faction in `AddActor`.
- **`Actor`** pawn fields:
  - Stable `Id`
  - `Position` (`SimVec2`)
  - Optional `Cell` when cell-anchored
  - `FactionId`
  - Tag-keyed **resources** (see [resources.md](resources.md)); health / max health / energy helpers
  - `ActorDefinition Definition`
  - Accessories and flat **`Effects`** cache (see [accessories.md](accessories.md))
  - `AbilityLoadout`, move intent consumed by world movement when an **`IMoveEffect`** is present
- **Movement collision:** each living actor that participates in movement is a circle of `PlayerRadius`. `ApplyMovement` slides against hex wall polygons **and** other living free actors (same radius); they cannot occupy overlapping circles. Dead actors are not obstacles.
- `GameWorld` holds a **mutable roster** of all actors plus a cell occupancy map (subset of the roster).
- Spawns take an **`ActorDefinition`** (normally `GameContent.DefaultActor`) and attach definition accessories via `AddAccessory`. World holds resource catalog + health tags from `GameContent` so actors can initialize and clamp health.
- **`FactionRules.AreHostile(int a, int b)`** → `a != b`. No other faction constants in this helper.
- **Spawn configuration** (bootstrap parameters only): `playerFactionId`, `rivalFactionId`, `aiPerFaction`, `humanPlayerCount`. Normal play: `GameSession` creates **`Player`** records and spawns their actors from `GameContent.DefaultActor` (plus lobby-selected accessories), then places **`spawnerCount`** intrinsic `zombie_spawner` actors (see [waves.md](../../../game/features/gameplay/waves.md)). `ScenarioRunner` runs the wave/level countdown by default; level regen re-places intrinsic spawners. Legacy `GameWorld.SpawnDefaultRoster` (humans + ally AI + rival AI) remains for tests. **Minimap.Client** (`ClientSession`) attaches `PlayerController`s associated with each `Player`.

## Death

When a destructible actor’s health resource ≤ 0, remove it quietly (game health doc): leave the roster, clear occupancy if cell-anchored, and unpossess/remove controllers. Before removal, apply any **`IDeathDropEffect`** on its effects (place the named actor definition on the corpse hex when empty).
