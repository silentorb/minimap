# Actors (technical)

Actor definition vs instance contracts. Implements [../../game/features/actors.md](../../game/features/actors.md). Related: [characters.md](characters.md), [characters-and-factions.md](characters-and-factions.md), [accessories.md](accessories.md), [cell-placement.md](cell-placement.md), [definition-config.md](definition-config.md), [depiction.md](depiction.md).

## Requirements

- **`ActorDefinition`** (Types): `Id`, ordered accessory definitions, optional `DepictionConfig` / `IconConfig` / `DisplayName`.
- **`CharacterDefinition` : `ActorDefinition`** — character content under `config/characters/`.
- **`Actor`** (Simulation): accessories, flat `Effects` cache, resource bag, facing, optional `DepictionOverride`, definition ref. `AddAccessory` / `RemoveAccessory` sync effects and run on-acquire effects.
- **`Character` : `Actor`** — faction, move intent, `AbilityLoadout`, health helpers; possessable pawn.
- Cell-anchored actors: `GameWorld` occupancy map cell → `Actor`; `TryPlaceActor` / `TryRemoveActorAt` / `IsCellOccupied`. Definitions catalog on the world from `GameContent.Actors`.
- JSON load order: **`actors/` → resources → accessories → characters** (see [definition-config.md](definition-config.md)). Registry APIs: `AddActorDefinition` / `ActorDefinitions` / `TryGetActorDefinition`.
- Client syncs cell actors separately from characters; applies depiction override when set.

## Non-goals (for now)

- Unified mobile+cell actor list API beyond characters list + cell map
