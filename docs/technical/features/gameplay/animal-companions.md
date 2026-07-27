# Animal companions (technical)

Implements [animal-companions.md](../../../game/features/gameplay/animal-companions.md). Related: [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [characters.md](characters.md), [characters-and-factions.md](characters-and-factions.md), [definition-config.md](../platform/definition-config.md).

## Requirements

- **`IWorldCharacterPassiveEffect`** — world-aware per-tick effect on characters. Dispatched from **`GameWorld.TickCharacterPassives`** alongside **`IPassiveEffect`**.
- **`GameWorld.TrySpawnNearbyActor`** — spawn one AI character on nearby grass (then all-grass fallback) with caller-chosen faction / aggression / `seekCrops` / optional **`ownerActorId`**. On success sets **`Actor.OwnerActorId`**. **`TrySpawnNearbyHostile`** remains a thin wrapper that passes the rival faction id (no owner).
- CompuQuest effect **`spawn_nearby_ally`** (`SpawnNearbyAllyEffect`):
  - JSON: `{ "type": "spawn_nearby_ally", "characterId": "<id>" }` (optional `aggression`; default **`AiTuning.DefaultAggression`**).
  - On tick: if not yet spawned, resolve `characterId` via **`GameWorld.TryGetActorDefinition`** (missing → fail-fast); call **`TrySpawnNearbyActor`** with the owner’s **`FactionId`**, owner axial from **`HexWorldLayout.WorldToAxial`**, and **`ownerActorId: actor.Id`**; on success mark done; on null retry later.
- Owned companion movement / injury flee toward owner are handled by shared **`AiController`** (see [controllers.md](controllers.md) and game [ai.md](../../../game/features/gameplay/ai.md)).
- Shipped accessories / characters under CompuQuest config: `fox`, `squid`, `monkey`, `penguin`, `poison_dart_frog` (accessories activation none + `player_selectable`; characters zombie-equivalent accessories + texture depiction).

## Failure strategy

- Missing / empty `characterId` at factory parse → fail-fast load.
- Missing actor definition at tick → fail-fast (content invariant).
- No grass hex for placement → expected; leave unsprung and retry (do not mark success).
