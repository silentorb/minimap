# Actors (technical)

Actor definition vs instance contracts. Implements [actors.md](../../../game/features/gameplay/actors.md). Related: [characters.md](characters.md), [characters-and-factions.md](characters-and-factions.md), [accessories.md](accessories.md), [health.md](../../../game/features/gameplay/health.md), [cell-placement.md](cell-placement.md), [definition-config.md](../platform/definition-config.md), [depiction.md](depiction.md).

## Requirements

- **`ActorDefinition`** (Types): `Id`, ordered accessory definitions, optional `DepictionConfig` / `IconConfig` / `DisplayName`, optional starting **`Resources`** (`TagId` → amount) applied when the actor is constructed.
- **`CharacterDefinition` : `ActorDefinition`** — character content under `config/characters/`.
- **`Actor`** (Simulation): stable **`Id`** from a shared world allocator (characters and cell placeables), accessories, flat `Effects` cache, resource bag, facing, **`FactionId`** (default **0**), optional `DepictionOverride`, definition ref, health helpers (`Health` / `MaxHealth` / `IsDestructible` / `IsAlive`). `AddAccessory` / `RemoveAccessory` sync effects and run on-acquire effects.
- **`Character` : `Actor`** — move intent, `AbilityLoadout`, energy helpers; possessable pawn. Sets `FactionId` in the constructor. Always initializes destructible health (default max 100) plus energy.
- Cell-anchored actors: `GameWorld` occupancy map cell → `Actor`; `TryPlaceActor(cell, definition, factionId)` / `TryRemoveActorAt` / `IsCellOccupied`. Definitions catalog on the world from `GameContent.Actors`. Dead destructible cell actors are pruned after damage.
- JSON load order: **`actors/` → resources → accessories → characters** (see [definition-config.md](../platform/definition-config.md)). Registry APIs: `AddActorDefinition` / `ActorDefinitions` / `TryGetActorDefinition`.
- Client syncs cell actors separately from characters; applies depiction override when set.

## Non-goals (for now)

- Unified mobile+cell actor list API beyond characters list + cell map
