# Combat

Missiles, shooting, and Swing. Related: [damage.md](damage.md), [health.md](health.md), [factions.md](factions.md), [ai.md](ai.md), [accessories.md](accessories.md), [characters.md](characters.md), [actors.md](actors.md), [local-input.md](../session/local-input.md), [active-abilities.md](active-abilities.md). Technical: [missiles-and-damage.md](../../../technical/features/gameplay/missiles-and-damage.md), [controllers.md](../../../technical/features/gameplay/controllers.md), [accessories.md](../../../technical/features/gameplay/accessories.md).

## Requirements

### Gun / missiles

- Actors shoot **missiles** when they have an **`IShootEffect`** on their effect cache (CompuQuest **`ShootEffect`**) and can afford that effect’s use cost (Gun / computer gun: **1 ammo**). Controllers only fire for characters that have an `IShootEffect`. Cell-anchored **computers** auto-aim and fire via a world-passive tick (no controller).
- Each shot spawns a free **projectile actor** of the type named by the shoot effect (`projectileActorId`; shipped **`missile`**). The gun assigns flight attributes at fire time (velocity, damage, friendly fire, range, effective size).
- **Missile speed**: shipped Gun / computer gun **400** world units per second (character move speed is 120) — from the shoot effect.
- **Missile range**: max **distance traveled** in world units (shipped **800**). Independent of speed — the projectile despawns when distance traveled reaches range (walls and hits still end flight early).
- **Missile size**: **base** collision radius comes from the selected projectile actor definition (`size`; shipped missile ≈ **12.285**, three times the former hard-coded missile radius). The gun may set an optional **`missileSizeScale`** (default **1**); effective radius = base × scale.
- **Fire direction**:
  - **Player**: aim via right stick / mouse; fire only while **primary fire** is held (RT / LMB). If aim is zero, fall back to character **facing**. Gun is a **dedicated** ability bind (`primary_fire`), not modal.
  - **AI / computer turrets**: aim toward the **nearest living hostile** (different faction), or do not fire when none exists. Projectiles are not valid aim targets.
- **Fire interval**: about **1.25** seconds between shots — cooldown lives on the **`IShootEffect`**, not on the controller.
- Missiles that hit a living **destructible** non-projectile actor (other than the shooter) apply damage per [damage.md](damage.md) (default Gun damage **25**). **Gun / `ShootEffect` defaults to friendly fire** for factioned targets. Missiles that hit walls or reach range are destroyed. Cell-anchored destructible actors are valid missile targets. Geek-placed computers inherit the placer’s faction and start with **10** ammo.

### Swing

- **Swing** is a short-range frontal **half-circle** attack (one of potentially several melee-style attacks). Characters swing when they have an **`ISwingEffect`** (CompuQuest **`SwingEffect`** via the **Swing** ability).
- **Attack direction** is supplied by the controller: aim (else facing), same as Gun.
- **Player**: Swing fires while **secondary fire** is held (LT / RMB). Swing is a **dedicated** bind (`secondary_fire`), not modal.
- **AI**: aims toward the nearest living hostile and swings when that hostile is within Swing **radius**.
- **Defaults**: damage **30**, interval **0.8** s, radius equal to map **hex size** (default **26**), arc **180°**, brief visual duration **0.15** s, friendly fire **on** for character targets, use cost **1 energy**.
- On fire, the simulation spawns a short-lived **Swing arc** for client visualization and **immediately** resolves overlaps once (not a lingering damage volume). Any destructible actor whose body overlaps the half-disk is damaged (except the attacker); characters honor friendly fire; cell actors always if destructible.

## Non-goals (for now)

- Multiple gun weapon types or charge shots
- Ways to refill ammo after the Gun’s starting grant (see [resources.md](resources.md))
- Homing missiles or splash damage
- Additional melee attacks beyond Swing (future; Swing establishes the pattern)
