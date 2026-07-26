# Animal companions (technical)

Implements [animal-companions.md](../../../game/features/gameplay/animal-companions.md). Related: [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [characters.md](characters.md), [characters-and-factions.md](characters-and-factions.md), [definition-config.md](../platform/definition-config.md).

## Requirements

- **`IWorldCharacterPassiveEffect`** — world-aware per-tick effect on characters. Dispatched from **`GameWorld.TickCharacterPassives`** alongside **`IPassiveEffect`**.
- **`GameWorld.TrySpawnNearbyCharacter`** — spawn one AI character on nearby grass (then all-grass fallback) with caller-chosen faction / aggression / `seekCrops`. **`TrySpawnNearbyHostile`** remains a thin wrapper that passes the rival faction id.
- CompuQuest effect **`spawn_nearby_ally`** (`SpawnNearbyAllyEffect`):
  - JSON: `{ "type": "spawn_nearby_ally", "characterId": "<id>" }` (optional `aggression`; default **`AiTuning.DefaultAggression`**).
  - On tick: if not yet spawned, resolve `characterId` via **`GameWorld.TryGetCharacterDefinition`** (missing → fail-fast); call **`TrySpawnNearbyCharacter`** with the owner’s **`FactionId`** and owner axial from **`HexWorldLayout.WorldToAxial`**; on success mark done; on null retry later.
- Shipped accessories / characters under CompuQuest config: `fox`, `squid`, `monkey`, `penguin`, `poison_dart_frog` (accessories activation none + `player_selectable`; characters zombie-equivalent accessories + texture depiction).

## Failure strategy

- Missing / empty `characterId` at factory parse → fail-fast load.
- Missing character definition at tick → fail-fast (content invariant).
- No grass hex for placement → expected; leave unsprung and retry (do not mark success).
