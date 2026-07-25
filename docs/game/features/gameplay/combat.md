# Combat

Missiles, shooting, and Swing. Related: [damage.md](damage.md), [health.md](health.md), [factions.md](factions.md), [ai.md](ai.md), [accessories.md](accessories.md), [characters.md](characters.md), [actors.md](actors.md), [local-input.md](../session/local-input.md), [active-abilities.md](active-abilities.md). Technical: [missiles-and-damage.md](../../../technical/features/gameplay/missiles-and-damage.md), [controllers.md](../../../technical/features/gameplay/controllers.md), [accessories.md](../../../technical/features/gameplay/accessories.md).

## Requirements

### Gun / missiles

- Characters shoot **medium-speed missiles** when they have an **`IShootEffect`** on their character effect cache (CompuQuest **`ShootEffect`** via the **Gun** ability) and can afford that effect’s use cost (Gun: **1 ammo**). Controllers only fire if the pawn has an `IShootEffect`.
- **Missile speed**: **200** world units per second (character move speed is 120) — from the effect.
- **Fire direction** is supplied by the controller (not by the effect):
  - **Player**: aim via right stick / mouse; fire only while **primary fire** is held (RT / LMB). If aim is zero, fall back to character **facing**. Gun is a **dedicated** ability bind (`primary_fire`), not modal.
  - **AI**: aim toward the **nearest living hostile** (different faction), or do not fire when none exists.
- **Fire interval**: about **1.25** seconds between shots — cooldown lives on the **`IShootEffect`**, not on the controller.
- Missiles that hit a living **destructible** actor (other than the shooter) apply damage per [damage.md](damage.md) (default Gun damage **25**). **Gun / `ShootEffect` defaults to friendly fire** for character targets. Missiles that hit walls are destroyed. Cell-anchored destructible actors are valid missile targets.

### Swing

- **Swing** is a short-range frontal **half-circle** attack (one of potentially several melee-style attacks). Characters swing when they have an **`ISwingEffect`** (CompuQuest **`SwingEffect`** via the **Swing** ability).
- **Attack direction** is supplied by the controller: aim (else facing), same as Gun.
- **Player**: Swing fires while **secondary fire** is held (LT / RMB). Swing is a **dedicated** bind (`secondary_fire`), not modal.
- **AI**: aims toward the nearest living hostile and swings when that hostile is within Swing **radius**.
- **Defaults**: damage **30**, interval **0.8** s, radius equal to map **hex size** (default **26**), arc **180°**, brief visual duration **0.15** s, friendly fire **on** for character targets, no resource cost.
- On fire, the simulation spawns a short-lived **Swing arc** for client visualization and **immediately** resolves overlaps once (not a lingering damage volume). Any destructible actor whose body overlaps the half-disk is damaged (except the attacker); characters honor friendly fire; cell actors always if destructible.

## Non-goals (for now)

- Multiple gun weapon types or charge shots
- Ways to refill ammo after the Gun’s starting grant (see [resources.md](resources.md))
- Homing missiles or splash damage
- Additional melee attacks beyond Swing (future; Swing establishes the pattern)
