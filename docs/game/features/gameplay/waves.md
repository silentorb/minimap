# Waves

Wave timing and spawner behavior. Related: [scenarios.md](../session/scenarios.md), [ai.md](ai.md), [factions.md](factions.md), [characters.md](characters.md).

## Status

**Disabled in normal play.** Wave pacing and level-end-via-last-wave are parked behind `ScenarioRunner.Enabled` (default `false`). `GameSession.Create` does **not** place spawners. The simulation APIs and scenario JSON fields remain for tests and a later return.

## Parked requirements (when re-enabled)

- Each level begins with a **preparation phase** lasting `preparationDuration` seconds from the scenario. No waves fire during preparation.
- After preparation, **wave 1** starts immediately.
- Subsequent waves start every `waveDuration` seconds until `waveCount` waves have fired for the level.
- On each wave, **every spawner** on the map attempts to spawn `spawnerVolume` hostile AI characters on the rival faction, picking each character from that spawner’s **weighted character pool**. If the pool is empty, that spawner emits **nothing**.
- Enemies spawn on **nearby floor hexes** (within 2 axial steps of the spawner; same hex allowed as fallback).
- Spawners are **static map markers** placed at random floor hexes when a level initializes. Placement picks spawner definitions from the integrator-supplied **world spawner pool** (weighted). An empty world pool places no spawners. Markers persist for the level and are replaced on level transition.
- CompuQuest currently supplies a pool of **zombie spawners** (character pool: zombie with Gun).
- Player spawning is a **separate** step from spawner placement.
- When the last wave of a level has fired, the level ends and a new level begins (see [scenarios.md](../session/scenarios.md)).

## Future

- Prefer **per-spawner timers** (organic emission) over a global wave clock when enemies return.
- Wave announcements or UI
- Escalating spawner volume per wave

## Non-goals (for now)

- Re-enabling waves or placing spawners in sandbox sessions
