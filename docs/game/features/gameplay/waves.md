# Waves

Spawner placement, intrinsic emission, and the wave/level clock. Related: [scenarios.md](../session/scenarios.md), [ai.md](ai.md), [factions.md](factions.md), [characters.md](characters.md), [health.md](health.md).

## Live (normal play)

### Intrinsic spawners (rival pressure)

- At session start (and after each level regen), **`spawnerCount`** destructible **placeable** spawners are placed on random grass hexes (default **2**).
- CompuQuest spawners are the **`zombie_spawner`** actor: max/start health **400** (4× default character health), depiction via spawn-node icon.
- Emission is **intrinsic**: each spawner has a **`spawn`** accessory effect with configurable **`intervalSeconds`**, **`volume`** (units per pulse), and a weighted **character pool**. Default: interval **15** s, volume **2**, pool **zombie** (weight 2) and **zombie_farmer** (weight 1).
- On each pulse, the spawner emits up to `volume` rival-faction AI on nearby grass (within 2 axial steps; same hex allowed as fallback). Destroyed spawners stop emitting.
- Player spawning is a separate step from spawner placement.

### Wave / level clock

- The global **`ScenarioRunner`** is **enabled** by default: preparation → wave-interval countdown → level regen (see [scenarios.md](../session/scenarios.md)).
- Waves are **thin** for now: the clock advances through `waveCount` intervals of `waveDuration` after preparation. How waves should hook into combat, economy, or other systems is undecided.
- Normal sessions do **not** place marker spawners, so scheduled marker `SpawnWaveEnemies` bursts are a no-op in normal play (countdown only). Intrinsic placeables remain the rival source.

## Parked / future (marker wave bursts)

Marker spawners + `SpawnWaveEnemies` remain in code for tests and possible future wave events. Prefer not to combine marker bursts with live intrinsic placeables without an explicit design (would double-spawn).

## Non-goals (for now)

- Wave announcements or UI
- Escalating volume per pulse
- Richer wave events wired into farming, ammo, or abilities
