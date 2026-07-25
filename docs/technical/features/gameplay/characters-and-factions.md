# Characters and factions

Simulation character model and faction APIs. Implements [factions.md](../../../game/features/gameplay/factions.md) and [health.md](../../../game/features/gameplay/health.md). Related: [characters.md](characters.md), [accessories.md](accessories.md), [resources.md](resources.md), [controllers.md](controllers.md).

## Requirements

- **`Character`** (pawn) fields:
  - Stable `Id`
  - `Position` (`SimVec2`)
  - `FactionId` (`int`)
  - Tag-keyed **resources** (see [resources.md](resources.md)); health / max health are resources, not dedicated float fields
  - `CharacterDefinition Definition`
  - Accessories and flat **`Effects`** cache (see [accessories.md](accessories.md))
  - Move intent consumed by world movement
- **Movement collision:** each living character is a circle of `PlayerRadius`. `ApplyMovement` slides against hex wall polygons **and** other living characters (same radius); characters cannot occupy overlapping circles. Dead characters are not obstacles.
- `GameWorld` holds a **mutable roster** of characters (not a fixed 1–4 array).
- Spawns take a **`CharacterDefinition`** (normally `GameContent.DefaultCharacter`) and attach definition accessories via `AddAccessory`. World holds resource catalog + health tags from `GameContent` so characters can initialize and clamp health.
- **`FactionRules.AreHostile(int a, int b)`** → `a != b`. No other faction constants in this helper.
- **Spawn configuration** (bootstrap parameters only): `playerFactionId`, `rivalFactionId`, `aiPerFaction`, `humanPlayerCount`. Normal play: `GameSession` creates **`Player`** records and spawns their characters from `GameContent.DefaultCharacter` (plus lobby-selected accessories). **No spawners** are placed while waves are parked (see [waves.md](../../../game/features/gameplay/waves.md)). Legacy `GameWorld.SpawnDefaultRoster` (humans + ally AI + rival AI) remains for tests. **Minimap.Client** (`ClientSession`) attaches `PlayerController`s associated with each `Player`.

## Death

When health resource ≤ 0, remove the character from the roster and unpossess/remove its controller (quiet removal per game health doc).
