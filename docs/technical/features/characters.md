# Characters (technical)

Character definition vs instance contracts. Implements [../../game/features/characters.md](../../game/features/characters.md). Related: [actors.md](actors.md), [accessories.md](accessories.md), [characters-and-factions.md](characters-and-factions.md), [extensions.md](extensions.md), [definition-config.md](definition-config.md), [depiction.md](depiction.md).

## Requirements

- **`CharacterDefinition` : `ActorDefinition`** in **`Minimap.Simulation.Types`** (contracts only; see that project’s `AGENTS.md`).
- Runtime **`Character` : `Actor`** (Simulation) adds faction, move intent, `AbilityLoadout`, health helpers (see [accessories.md](accessories.md), [actors.md](actors.md)).
- Instantiation from a definition creates accessory instances (cloning effects), then **`AddAccessory`** each so the effect cache stays consistent.
- **`GameContent.DefaultCharacter`** is the definition used for normal spawns (humans, rivals, refill). App obtains `GameContent` from the active integrator and passes it into the session/world; consumers treat it as ordinary content, not as an “integration” object.
- Shipped short-term **generic** character is JSON under **`src/CompuQuest.Minimap/config/characters/generic.json`** (empty accessories list + sprite-frames depiction; copied to `extensions/CompuQuest.Minimap/` on build). Loaded by App into the registry (see [definition-config.md](definition-config.md)). Players receive selectable abilities only from lobby choices.
