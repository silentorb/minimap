# Combat

Missiles and shooting. Related: [damage.md](damage.md), [health.md](health.md), [factions.md](factions.md), [ai.md](ai.md), [accessories.md](accessories.md), [characters.md](characters.md), [local-input.md](local-input.md). Technical: [../../technical/features/missiles-and-damage.md](../../technical/features/missiles-and-damage.md), [../../technical/features/controllers.md](../../technical/features/controllers.md), [../../technical/features/accessories.md](../../technical/features/accessories.md).

## Requirements

- Characters shoot **medium-speed missiles** when they have a **`ShootEffect`** on their character effect cache (provided by the **Gun** accessory on the generic character definition). Controllers only fire if the pawn has a `ShootEffect`.
- **Missile speed**: **200** world units per second (character move speed is 120) — from the effect.
- **Fire direction** is supplied by the controller (not by the effect):
  - **Player**: twin-stick aim — fire in the aim-stick / aim-key direction while that vector is non-zero. No separate fire button.
  - **AI**: aim toward the **nearest living hostile** (different faction), or do not fire when none exists.
- **Fire interval**: about **1.25** seconds between shots — cooldown lives on the **`ShootEffect`**, not on the controller.
- Missiles that hit a living character (other than the shooter) apply damage per [damage.md](damage.md) (default Gun damage **25**). **`ShootEffect` / Gun defaults to friendly fire**, so same-faction overlap damages. Missiles that hit walls are destroyed.

## Non-goals (for now)

- Multiple weapon types, ammo, or charge shots
- Homing missiles or splash damage
