# Scenarios

Gameplay pacing and map-init configuration. Related: [waves.md](../gameplay/waves.md), [game-over.md](../ui/game-over.md). Technical: [scenario-settings.md](../../../technical/features/session/scenario-settings.md).

## Requirements

- A **scenario** defines how a playthrough is paced and how the map is initialized.
- Scenarios are loaded from **JSON files** at game start.
- Default scenario: **`config/scenarios/default.json`** (`res://config/scenarios/default.json`).
- A custom scenario file may be passed on the command line: `--scenario=<path>` or `--scenario <path>`.
- Scenario fields:
  - **preparationDuration** — seconds (float) of quiet time before the first wave interval
  - **waveCount** — number of wave intervals per level (int, ≥ 1)
  - **waveDuration** — seconds (float) between wave interval completions
  - **spawnerCount** — intrinsic placeable spawners placed on the map at session start and after each level regen (int, ≥ 1); default **2**
  - **spawnerVolume** — retained for marker wave-burst emission (int, ≥ 1); live rival emission uses each intrinsic spawner’s `spawn` effect `volume`
- **Normal play:** `ScenarioRunner.Enabled` defaults to **true**. After preparation and `waveCount` wave intervals, a **new level** begins: terrain regenerates (seed + level index), rivals are cleared, intrinsic spawners are re-placed, and all human players are healed and resurrected if dead. Wave intervals themselves do not yet fire additional content events (see [waves.md](../gameplay/waves.md)).
- At session start the map is populated with **human players** and **`spawnerCount`** destructible intrinsic spawners.
- Scaling difficulty per level is a future enhancement; `LevelIndex` is tracked for later use.
- **Intended (not built):** players configure scenario fields in the [lobby](../ui/lobby.md) wizard rather than via diegetic in-match influence. Until then, JSON / CLI / env remain the configuration path.

## Non-goals (for now)

- Per-level scenario overrides
- Difficulty scaling
- Lobby scenario UI (documented intent only)
- Diegetic scenario / mode configuration as a foundational feature
