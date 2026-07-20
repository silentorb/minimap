# Waves

Wave timing and spawner behavior. Related: [scenarios.md](scenarios.md), [ai.md](ai.md), [factions.md](factions.md).

## Requirements

- Each level begins with a **preparation phase** lasting `preparationDuration` seconds from the scenario. No waves fire during preparation.
- After preparation, **wave 1** starts immediately.
- Subsequent waves start every `waveDuration` seconds until `waveCount` waves have fired for the level.
- On each wave, **every wave spawner** on the map spawns `spawnerVolume` hostile AI characters on the rival faction.
- Enemies spawn on **nearby floor hexes** (within 2 axial steps of the spawner; same hex allowed as fallback).
- Wave spawners are **static map markers** placed at random floor hexes when a level initializes. They persist for the level and are replaced on level transition.
- When the last wave of a level has fired, the level ends and a new level begins (see [scenarios.md](scenarios.md)).

## Non-goals (for now)

- Wave announcements or UI
- Escalating spawner volume per wave
