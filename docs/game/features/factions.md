# Factions

Faction membership and hostility. Related: [ai.md](ai.md), [combat.md](combat.md), [damage.md](damage.md). Technical: [../../technical/features/characters-and-factions.md](../../technical/features/characters-and-factions.md).

## Requirements

- Each character is a member of **exactly one** faction, represented by a **numeric id** (`int`).
- **Hostility**: two characters are enemies if and only if their faction ids **differ**. Combat and AI target hostiles only; never hard-code “attack the player.”
- Core rules (hostility, targeting, friendly fire) must stay **generic**—no special-casing of particular faction numbers inside combat or AI logic.
- **Current game mode (surface only)**: two factions with ids **1** (player’s faction) and **2** (rival). Spawn:
  - **1** human-controlled character on faction **1**
  - **3** AI on faction **1**
  - **3** AI on faction **2**
- Specific ids **1** and **2**, and those counts, belong at **bootstrap/spawn configuration** only—not inside generalized faction helpers.

## Future

- Membership may become many-to-many; one faction per character is enough for now.
