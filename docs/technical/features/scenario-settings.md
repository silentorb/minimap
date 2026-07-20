# Scenario settings

JSON configuration for [scenarios](../../game/features/scenarios.md). Loaded at the **Minimap.App** surface.

## Requirements

- Default file: **`config/scenarios/default.json`** (Godot path `res://config/scenarios/default.json`).
- **Minimap.App** exposes `ScenarioSettings.LoadFromJson` and `ScenarioSettings.LoadFromFile`, returning `Minimap.Simulation.Scenario`.
- **CLI override:** `--scenario=<path>` or `--scenario <path>` (parsed by `CliArgs.TryGetScenarioPath`). `GameApp` uses CLI path when present, else the default file. The resolved path is stored on `LocalPlayContextNode.ScenarioPath` for game-over reload.
- Schema:

```json
{
  "preparationDuration": 10.0,
  "waveCount": 3,
  "waveDuration": 15.0,
  "spawnerCount": 4,
  "spawnerVolume": 2
}
```

- All numeric fields are required. `preparationDuration` and `waveDuration` must be **> 0**. `waveCount`, `spawnerCount`, and `spawnerVolume` must be **≥ 1**. Invalid JSON or values fail fast.
- Documented defaults match `Scenario.Defaults` in Simulation.
- Simulation owns the `Scenario` type; App performs file I/O only.

## Non-goals (for now)

- Hot-reload during play
- Multiple scenario files merged at runtime
