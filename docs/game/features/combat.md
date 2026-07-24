# Combat

Missiles and shooting. Related: [damage.md](damage.md), [health.md](health.md), [factions.md](factions.md), [ai.md](ai.md), [accessories.md](accessories.md), [characters.md](characters.md), [local-input.md](local-input.md), [active-abilities.md](active-abilities.md). Technical: [../../technical/features/missiles-and-damage.md](../../technical/features/missiles-and-damage.md), [../../technical/features/controllers.md](../../technical/features/controllers.md), [../../technical/features/accessories.md](../../technical/features/accessories.md).

## Requirements

- Characters shoot **medium-speed missiles** when they have an **`IShootEffect`** on their character effect cache (CompuQuest **`ShootEffect`** via the **Gun** ability) and can afford that effect’s use cost (Gun: **1 ammo**). Controllers only fire if the pawn has an `IShootEffect`.
- **Missile speed**: **200** world units per second (character move speed is 120) — from the effect.
- **Fire direction** is supplied by the controller (not by the effect):
  - **Player**: aim via right stick / mouse; fire only while **primary fire** is held (RT / LMB). If aim is zero, fall back to character **facing**. Gun is a **dedicated** ability bind (`primary_fire`), not modal.
  - **AI**: aim toward the **nearest living hostile** (different faction), or do not fire when none exists.
- **Fire interval**: about **1.25** seconds between shots — cooldown lives on the **`IShootEffect`**, not on the controller.
- Missiles that hit a living character (other than the shooter) apply damage per [damage.md](damage.md) (default Gun damage **25**). **Gun / `ShootEffect` defaults to friendly fire**, so same-faction overlap damages. Missiles that hit walls are destroyed.

## Non-goals (for now)

- Multiple weapon types or charge shots
- Ways to refill ammo after the Gun’s starting grant (see [resources.md](resources.md))
- Homing missiles or splash damage
