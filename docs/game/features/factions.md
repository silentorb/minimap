# Factions

Faction membership and hostility. Related: [ai.md](ai.md), [combat.md](combat.md), [damage.md](damage.md). Technical: [../../technical/features/characters-and-factions.md](../../technical/features/characters-and-factions.md).

## Requirements

- Each character is a member of **exactly one** faction, represented by a **numeric id** (`int`).
- **Hostility**: two characters are enemies if and only if their faction ids **differ**. Combat and AI target hostiles only; never hard-code “attack the player.”
- Core rules (hostility, targeting, friendly fire) must stay **generic**—no special-casing of particular faction numbers inside combat or AI logic.
- **Current game mode (surface only)**: two factions with ids **1** (player’s faction) and **2** (rival). Spawn:
  - **Human players** on faction **1** (count from lobby / local play)
  - **Rival AI**: none in normal sandbox play (spawners / waves parked; see [waves.md](waves.md))
- The legacy bootstrap roster (1 human + 3 ally AI + 3 rival AI) remains available as `GameWorld.SpawnDefaultRoster` for tests but is **not** used in normal play.
- Specific ids **1** and **2**, and those counts, belong at **bootstrap/spawn configuration** only—not inside generalized faction helpers.

## Future

- Membership may become many-to-many; one faction per character is enough for now.
