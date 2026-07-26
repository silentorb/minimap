# Scenarios

Gameplay pacing and map-init configuration. Related: [waves.md](../gameplay/waves.md), [game-over.md](../ui/game-over.md). Technical: [scenario-settings.md](../../../technical/features/session/scenario-settings.md).

## Requirements

- A **scenario** defines how a playthrough is paced and how the map is initialized.
- Scenarios are loaded from **JSON files** at game start.
- Default scenario: **`config/scenarios/default.json`** (`res://config/scenarios/default.json`).
- A custom scenario file may be passed on the command line: `--scenario=<path>` or `--scenario <path>`.
- Scenario fields:
  - **preparationDuration** — seconds (float) of quiet time before the first wave (parked wave runner)
  - **waveCount** — number of waves per level (int, ≥ 1) (parked wave runner)
  - **waveDuration** — seconds (float) between wave starts (parked wave runner)
  - **spawnerCount** — intrinsic placeable spawners placed on the map at session start (int, ≥ 1); default **2**
  - **spawnerVolume** — retained for parked wave emission (int, ≥ 1); live emission uses each spawner’s `spawn` effect `volume`
- **Sandbox (current normal play):** one **persistent map** per session. Level regeneration is **disabled** (`ScenarioRunner.Enabled` defaults to false). At session start the map is populated with **human players** and **`spawnerCount`** destructible intrinsic spawners (see [waves.md](../gameplay/waves.md)).
- Scaling difficulty per level is a future enhancement; `LevelIndex` is tracked for later use when pacing returns.

## Parked (when wave runner is re-enabled)

- After all waves in a level complete, a **new level** begins: terrain regenerates (seed + level index), rivals are cleared, spawners reposition, and all human players are healed and resurrected if dead.

## Non-goals (for now)

- Per-level scenario overrides
- Difficulty scaling
- Auto level transitions in sandbox sessions
