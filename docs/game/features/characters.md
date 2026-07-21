# Characters

Character definitions vs runtime instances. Related: [factions.md](factions.md), [health.md](health.md), [accessories.md](accessories.md), [combat.md](combat.md). Technical: [../../technical/features/characters.md](../../technical/features/characters.md), [../../technical/features/characters-and-factions.md](../../technical/features/characters-and-factions.md).

## Requirements

- A **character** is a runtime pawn (health, position, faction, accessories/effects) plus a **character definition** that describes what to spawn.
- Character definitions list **accessory definitions** applied when the character is instantiated.
- Short-term: one **generic** character definition (`config/characters/generic.json`) is used for all spawned characters (humans and rivals). That definition includes the **Gun** accessory (see [accessories.md](accessories.md), [combat.md](combat.md)).
- Playthrough content supplies a **default character** definition used when populating the world.

## Non-goals (for now)

- Player-facing character selection or customization
- Multiple distinct character archetypes in content
- Character stats (future; accessories/effects are shaped to support them later)
