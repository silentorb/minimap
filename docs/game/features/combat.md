# Combat

Missiles and shooting. Related: [damage.md](damage.md), [health.md](health.md), [factions.md](factions.md), [ai.md](ai.md). Technical: [../../technical/features/missiles-and-damage.md](../../technical/features/missiles-and-damage.md), [../../technical/features/controllers.md](../../technical/features/controllers.md).

## Requirements

- Characters shoot **medium-speed missiles** at hostiles.
- **Missile speed**: **200** world units per second (character move speed is 120).
- **Autoshoot**: aim and fire toward the **nearest living hostile** (different faction). No manual aim required.
- **Fire interval**: about **1.25** seconds between shots per character (cooldown).
- Autoshoot is shared by **player-controlled** and **AI-controlled** characters; the player steers while fire is automatic.
- Missiles that hit a hostile apply damage per [damage.md](damage.md). Missiles that hit walls are destroyed. Same-faction overlap does not damage.

## Non-goals (for now)

- Multiple weapon types, ammo, or charge shots
- Homing missiles or splash damage
