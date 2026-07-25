# Characters

Character definitions vs runtime instances. Related: [actors.md](actors.md), [factions.md](factions.md), [health.md](health.md), [accessories.md](accessories.md), [combat.md](combat.md). Technical: [characters.md](../../../technical/features/gameplay/characters.md), [characters-and-factions.md](../../../technical/features/gameplay/characters-and-factions.md).

## Requirements

- A **character** is an **actor** specialized as a possessable mobile pawn (resources including health, position, faction, accessories/effects, ability loadout) plus a **character definition** that describes what to spawn. See [actors.md](actors.md).
- Character definitions list **accessory definitions** applied when the character is instantiated, and may include a **depiction** (see [depiction.md](depiction.md)).
- Short-term: one **generic** character definition (`src/CompuQuest.Minimap/config/characters/generic.json`) is used for all spawned characters (humans and rivals). That definition has **no** starting accessories — players obtain abilities (e.g. **Gun**, **Farm**, **Geek**) only via lobby selection (see [accessories.md](accessories.md), [lobby.md](../ui/lobby.md)).
- Playthrough content supplies a **default character** definition used when populating the world.
- Wave enemies (e.g. zombie) may still list accessories on their character definition.

## Non-goals (for now)

- Player-facing character selection or customization
- Multiple distinct character archetypes in content
- Character stats beyond resources (future; accessories/effects are shaped to support them later)
