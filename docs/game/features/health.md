# Health

Character hit points. Related: [damage.md](damage.md), [combat.md](combat.md), [factions.md](factions.md).

## Requirements

- Every character has **current health** and **max health**.
- Default **max health** (and starting health) is **100**.
- When health reaches **0 or below**, the character **dies**.
- Death **quietly removes** the character from the map: no death animation, VFX, or UI for now. The character (and its controller) are gone on the next simulation update.

## Non-goals (for now)

- Health UI / bars
- Regeneration, armor, or invulnerability frames
