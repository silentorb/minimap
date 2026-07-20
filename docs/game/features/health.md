# Health

Character hit points. Related: [damage.md](damage.md), [combat.md](combat.md), [factions.md](factions.md).

## Requirements

- Every character has **current health** and **max health**.
- Default **max health** (and starting health) is **100**.
- When health reaches **0 or below**, the character **dies**.
- Death **quietly removes** the character from the map: no death animation, VFX, or UI for now. The character (and its controller) are gone on the next simulation update.
- **Level transition** (after all waves in a level): all human players are **healed to max health** and **resurrected** if they had died during the level.
- **Game over** (all human players dead): see [game-over.md](game-over.md) for the pause overlay exception.

## Non-goals (for now)

- Regeneration, armor, or invulnerability frames

Player health is shown on the local HUD; see [player-hud.md](player-hud.md).
