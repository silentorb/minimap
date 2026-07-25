# Scenarios

Gameplay pacing and map-init configuration. Related: [waves.md](../gameplay/waves.md), [game-over.md](../ui/game-over.md). Technical: [scenario-settings.md](../../../technical/features/session/scenario-settings.md).

## Requirements

- A **scenario** defines how a playthrough is paced and how the map is initialized.
- Scenarios are loaded from **JSON files** at game start.
- Default scenario: **`config/scenarios/default.json`** (`res://config/scenarios/default.json`).
- A custom scenario file may be passed on the command line: `--scenario=<path>` or `--scenario <path>`.
- Scenario fields (retained for the parked wave runner; unused in normal sandbox play):
  - **preparationDuration** — seconds (float) of quiet time before the first wave
  - **waveCount** — number of waves per level (int, ≥ 1)
  - **waveDuration** — seconds (float) between wave starts
  - **spawnerCount** — wave spawners placed on the map at level init (int, ≥ 1)
  - **spawnerVolume** — enemies spawned per spawner per wave (int, ≥ 1)
- **Sandbox (current normal play):** one **persistent map** per session. Level regeneration is **disabled** (`ScenarioRunner.Enabled` defaults to false). At session start the map is populated with **human players only**—**no spawners** are placed.
- Scaling difficulty per level is a future enhancement; `LevelIndex` is tracked for later use when pacing returns.

## Parked (when wave runner is re-enabled)

- At level start, wave spawners are placed per `spawnerCount`.
- After all waves in a level complete, a **new level** begins: terrain regenerates (seed + level index), rivals are cleared, spawners reposition, and all human players are healed and resurrected if dead.

## Non-goals (for now)

- Per-level scenario overrides
- Difficulty scaling
- Auto level transitions in sandbox sessions
