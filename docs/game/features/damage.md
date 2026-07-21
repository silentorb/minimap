# Damage

How health is reduced. Related: [health.md](health.md), [combat.md](combat.md), [factions.md](factions.md).

## Requirements

- Damage subtracts from the target’s current health (see [health.md](health.md)).
- A successful **missile hit** deals **25** damage.
- By default, missile damage applies to **any** living character except the shooter (**friendly fire on**). Same-faction characters take damage from each other’s missiles. Attacks may opt out (`FriendlyFire = false`) to damage hostiles only.
- Health is clamped at a minimum of 0 after damage; reaching 0 triggers death removal per [health.md](health.md).

## Non-goals (for now)

- Damage types, crits, knockback, or DoTs
- Environmental / hazard damage (terrain hazards may exist visually but are out of scope here)
