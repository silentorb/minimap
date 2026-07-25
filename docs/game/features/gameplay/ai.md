# AI

AI-controlled characters. Related: [combat.md](combat.md), [factions.md](factions.md), [health.md](health.md). Technical control: [controllers.md](../../../technical/features/gameplay/controllers.md), [navigation.md](../../../technical/features/gameplay/navigation.md).

## Requirements

- AI characters **wander**: they periodically pick a new random **grass hex world goal** (or briefly pause). Movement uses the same rules as any other character ([movement.md](movement.md): cartesian motion, wall-slide and character-circle collision). Toward a goal, move intent comes from **navigation steering** (direct or pathfinding/crowd—see technical navigation docs).
- AI characters **engage hostiles** using the same combat rules as a player-controlled character from the character’s point of view ([combat.md](combat.md)): if they have an `IShootEffect`, they fire with aim toward the nearest hostile; if they have an `ISwingEffect`, they aim the same way and swing when that hostile is within Swing radius. Wave zombies use Swing (not Gun).

- AI does **not** hard-code “attack the player.” Hostiles are defined by [factions.md](factions.md).
- Each game places AI on **both** factions used in the current mode (see spawn surface in [factions.md](factions.md)): **3** AI per faction by default, plus one human-controlled character on the player faction.

## Non-goals (for now)

- Cover, retreat, or coordinated group tactics
- Difficulty tiers or per-AI personality
