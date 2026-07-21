# AI

AI-controlled characters. Related: [combat.md](combat.md), [factions.md](factions.md), [health.md](health.md). Technical control: [../../technical/features/controllers.md](../../technical/features/controllers.md).

## Requirements

- AI characters **wander**: they periodically pick a new random move direction (or briefly pause), using the same movement rules as any other character (cartesian motion, wall-slide).
- AI characters **engage hostiles** using the same combat rules as a player-controlled character from the character’s point of view: [combat.md](combat.md) `IShootEffect` firing, with aim toward the nearest hostile faction member.
- AI does **not** hard-code “attack the player.” Hostiles are defined by [factions.md](factions.md).
- Each game places AI on **both** factions used in the current mode (see spawn surface in [factions.md](factions.md)): **3** AI per faction by default, plus one human-controlled character on the player faction.

## Non-goals (for now)

- Pathfinding, cover, retreat, or coordinated group tactics
- Difficulty tiers or per-AI personality
