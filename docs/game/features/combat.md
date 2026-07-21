# Combat

Missiles and shooting. Related: [damage.md](damage.md), [health.md](health.md), [factions.md](factions.md), [ai.md](ai.md), [accessories.md](accessories.md), [characters.md](characters.md). Technical: [../../technical/features/missiles-and-damage.md](../../technical/features/missiles-and-damage.md), [../../technical/features/controllers.md](../../technical/features/controllers.md), [../../technical/features/accessories.md](../../technical/features/accessories.md).

## Requirements

- Characters shoot **medium-speed missiles** at hostiles when they have an **`AutoshootEffect`** on their character effect cache (provided by the **Gun** accessory on the generic character definition).
- **Missile speed**: **200** world units per second (character move speed is 120) — from the effect.
- **Autoshoot**: aim and fire toward the **nearest living hostile** (different faction). No manual aim required.
- **Fire interval**: about **1.25** seconds between shots — cooldown lives on the **`AutoshootEffect`**, not on the controller.
- Autoshoot is shared by **player-controlled** and **AI-controlled** characters; the player steers while fire is automatic. Controllers only fire if the pawn has an `AutoshootEffect`.
- Missiles that hit a hostile apply damage per [damage.md](damage.md) (default Gun damage **25**). Missiles that hit walls are destroyed. Same-faction overlap does not damage.

## Non-goals (for now)

- Multiple weapon types, ammo, or charge shots
- Homing missiles or splash damage
