# Characters (technical)

Character definition vs instance contracts. Implements [../../game/features/characters.md](../../game/features/characters.md). Related: [accessories.md](accessories.md), [characters-and-factions.md](characters-and-factions.md), [extensions.md](extensions.md), [definition-settings.md](definition-settings.md).

## Requirements

- Definition and content types live in **`Minimap.Simulation.Types`** (little/no logic): `CharacterDefinition` (`Id`, ordered accessory definitions).
- Runtime **`Character`** (Simulation) holds definition, accessories, and a flat **`Effects`** cache (see [accessories.md](accessories.md)).
- Instantiation from a definition creates accessory instances (cloning effects), then **`AddAccessory`** each so the effect cache stays consistent.
- **`GameContent.DefaultCharacter`** is the definition used for normal spawns (humans, rivals, refill). App obtains `GameContent` from the active integrator and passes it into the session/world; consumers treat it as ordinary content, not as an “integration” object.
- Shipped short-term **generic** character is JSON under **`config/characters/generic.json`** (includes the Gun accessory). Loaded by App into the registry (see [definition-settings.md](definition-settings.md)).
