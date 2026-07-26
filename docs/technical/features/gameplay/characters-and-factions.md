# Characters and factions

Simulation character model and faction APIs. Implements [factions.md](../../../game/features/gameplay/factions.md) and [health.md](../../../game/features/gameplay/health.md). Related: [characters.md](characters.md), [accessories.md](accessories.md), [resources.md](resources.md), [controllers.md](controllers.md).

## Requirements

- **`Character`** (pawn) fields:
  - Stable `Id` (from **`Actor`**)
  - `Position` (`SimVec2`)
  - `FactionId` (`int`)
  - Tag-keyed **resources** on **`Actor`** (see [resources.md](resources.md)); health / max health helpers live on **`Actor`**
  - `CharacterDefinition Definition`
  - Accessories and flat **`Effects`** cache (see [accessories.md](accessories.md))
  - Move intent consumed by world movement
- **Movement collision:** each living character is a circle of `PlayerRadius`. `ApplyMovement` slides against hex wall polygons **and** other living characters (same radius); characters cannot occupy overlapping circles. Dead characters are not obstacles.
- `GameWorld` holds a **mutable roster** of characters (not a fixed 1–4 array) plus cell-anchored actors.
- Spawns take a **`CharacterDefinition`** (normally `GameContent.DefaultCharacter`) and attach definition accessories via `AddAccessory`. World holds resource catalog + health tags from `GameContent` so characters can initialize and clamp health.
- **`FactionRules.AreHostile(int a, int b)`** → `a != b`. No other faction constants in this helper.
- **Spawn configuration** (bootstrap parameters only): `playerFactionId`, `rivalFactionId`, `aiPerFaction`, `humanPlayerCount`. Normal play: `GameSession` creates **`Player`** records and spawns their characters from `GameContent.DefaultCharacter` (plus lobby-selected accessories). **No spawners** are placed while waves are parked (see [waves.md](../../../game/features/gameplay/waves.md)). Legacy `GameWorld.SpawnDefaultRoster` (humans + ally AI + rival AI) remains for tests. **Minimap.Client** (`ClientSession`) attaches `PlayerController`s associated with each `Player`.

## Death

When a destructible actor’s health resource ≤ 0, remove it quietly (game health doc): characters leave the roster and unpossess/remove controllers; cell actors leave occupancy. Before removing a character, apply any **`IDeathDropEffect`** on its effects (place the named actor definition on the corpse hex when empty).
