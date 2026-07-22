# Characters and factions

Simulation character model and faction APIs. Implements [../../game/features/factions.md](../../game/features/factions.md) and [../../game/features/health.md](../../game/features/health.md). Related: [characters.md](characters.md), [accessories.md](accessories.md), [controllers.md](controllers.md).

## Requirements

- **`Character`** (pawn) fields:
  - Stable `Id`
  - `Position` (`SimVec2`)
  - `FactionId` (`int`)
  - `Health` / `MaxHealth` (default max **100**)
  - `CharacterDefinition Definition`
  - Accessories and flat **`Effects`** cache (see [accessories.md](accessories.md))
  - Move intent consumed by world movement
- **Movement collision:** each living character is a circle of `PlayerRadius`. `ApplyMovement` slides against hex wall polygons **and** other living characters (same radius); characters cannot occupy overlapping circles. Dead characters are not obstacles.
- `GameWorld` holds a **mutable roster** of characters (not a fixed 1–4 array).
- Spawns take a **`CharacterDefinition`** (normally `GameContent.DefaultCharacter`) and attach definition accessories via `AddAccessory`.
- **`FactionRules.AreHostile(int a, int b)`** → `a != b`. No other faction constants in this helper.
- **Spawn configuration** (bootstrap parameters only): `playerFactionId`, `rivalFactionId`, `aiPerFaction`, `humanPlayerCount`. Normal play: `GameSession` creates **`Player`** records, spawns their characters from `GameContent.DefaultCharacter` (plus lobby-selected accessories), then places spawners from `GameContent.WorldSpawnerPool`. Legacy `GameWorld.SpawnDefaultRoster` (humans + ally AI + rival AI) remains for tests. **Minimap.Client** (`ClientSession`) attaches `PlayerController`s associated with each `Player`. Rival AI during play comes from spawners’ character pools (see [../../game/features/waves.md](../../game/features/waves.md)).

## Death

When health ≤ 0, remove the character from the roster and unpossess/remove its controller (quiet removal per game health doc).
