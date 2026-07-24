# Health

Character hit points as resources. Related: [resources.md](resources.md), [damage.md](damage.md), [combat.md](combat.md), [factions.md](factions.md), [hunger.md](hunger.md).

## Requirements

- Every character has **current health** (`health` resource) and **max health** (`max_health` resource). Health is limited by max health.
- Default **max health** (and starting health) is **100**.
- When health reaches **0 or below**, the character **dies**.
- Death **quietly removes** the character from the map: no death animation, VFX, or UI for now. The character (and its controller) are gone on the next simulation update.
- **Level transition** (after all waves in a level, when the parked wave runner is enabled): all human players are **healed to max health** and **resurrected** if they had died during the level. Inactive in normal sandbox play (see [scenarios.md](scenarios.md)).
- **Game over** (all human players dead): see [game-over.md](game-over.md) for the pause overlay exception.

## Non-goals (for now)

- Armor or invulnerability frames

Vitality from [hunger.md](hunger.md) can heal or hurt health over time based on energy bands. Player health is shown on the local HUD as part of the resource list; see [player-hud.md](player-hud.md) and [resources.md](resources.md).
