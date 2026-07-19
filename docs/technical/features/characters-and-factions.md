# Characters and factions

Simulation character model and faction APIs. Implements [../../game/features/factions.md](../../game/features/factions.md) and [../../game/features/health.md](../../game/features/health.md). Related: [controllers.md](controllers.md).

## Requirements

- **`Character`** (pawn) fields:
  - Stable `Id`
  - `Position` (`SimVec2`)
  - `FactionId` (`int`)
  - `Health` / `MaxHealth` (default max **100**)
  - Move intent consumed by world movement
- `GameWorld` holds a **mutable roster** of characters (not a fixed 1–4 array).
- **`FactionRules.AreHostile(int a, int b)`** → `a != b`. No other faction constants in this helper.
- **Spawn configuration** (bootstrap parameters only): `playerFactionId`, `rivalFactionId`, `aiPerFaction`, `humanPlayerCount` (default **1**). Simulation spawns that many **unpossessed** human-faction characters plus AI with `AiController`s. **Minimap.App** attaches Client `PlayerController`s to the humans. Default mode values (**1**, **2**, **3**) for factions / AI counts are configured at the spawn surface (`GameWorld.Create` / App exports), not inside `FactionRules` or autoshoot.

## Death

When health ≤ 0, remove the character from the roster and unpossess/remove its controller (quiet removal per game health doc).
