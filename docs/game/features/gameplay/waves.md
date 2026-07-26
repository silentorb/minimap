# Waves

Spawner placement and emission. Related: [scenarios.md](../session/scenarios.md), [ai.md](ai.md), [factions.md](factions.md), [characters.md](characters.md), [health.md](health.md).

## Live (sandbox)

- At session start, **`spawnerCount`** destructible **placeable** spawners are placed on random grass hexes (default **2**).
- CompuQuest spawners are the **`zombie_spawner`** actor: max/start health **400** (4× default character health), depiction via spawn-node icon.
- Emission is **intrinsic**: each spawner has a **`spawn`** accessory effect with configurable **`intervalSeconds`**, **`volume`** (units per pulse), and a weighted **character pool**. Default: interval **15** s, volume **2**, pool **zombie** (weight 2) and **zombie_farmer** (weight 1).
- On each pulse, the spawner emits up to `volume` rival-faction AI on nearby grass (within 2 axial steps; same hex allowed as fallback). Destroyed spawners stop emitting.
- Player spawning is a separate step from spawner placement.
- The global **`ScenarioRunner`** wave clock (preparation → waves → level regen) remains **parked** (`Enabled` defaults `false`) so sandbox maps stay persistent and do not double-fire from a wave schedule.

## Parked (global wave clock)

When `ScenarioRunner.Enabled` is opted in (tests / future meta-loop):

- Preparation, `waveCount`, `waveDuration`, and marker-based `SpawnWaveEnemies` still exist.
- Prefer not to combine that path with live intrinsic placeables without an explicit design (would double-spawn).

## Non-goals (for now)

- Wave announcements or UI
- Escalating volume per pulse
- Re-enabling level regeneration in sandbox sessions
