# Core settings

Shipped JSON configuration for developer-oriented (and user-editable) core game parameters. Loaded at the **Minimap.App** surface. Implements defaults for [map layout](../../game/features/map-layout.md) / [hex grid shape](hex-grid-shape.md) and [players](../../game/features/players.md) accessory budgets.

## Requirements

- Core settings live in **`config/core.json`** (Godot path `res://config/core.json`).
- This file is distinct from a future **user settings** file (preferences that are not core bootstrap parameters).
- **Minimap.App** exposes load APIs: `CoreSettings.LoadFromJson` and `CoreSettings.LoadFromFile`. Simulation has **no I/O**. Client does not open settings files (uses `WorldHostHooks` registered by App).
- `WorldApp` / `LobbyApp` load core settings on ready via an exportable path (default `res://config/core.json`) through host hooks.
- **Boot failure**: on any exception during world/lobby ready (including core settings load), abort via scene boot, `GD.PushError`, and quit—do not leave a half-initialized scene interactive.
- **Vector convention:** paired numeric settings prefer a 2D vector. In JSON they are a length-2 numeric array `[n, n]`, deserialized to `SimVec2I`.
- Current schema:

```json
{
  "map": {
    "radius": [8, 6]
  },
  "player": {
    "accessoryPoints": 2
  }
}
```

  - `radius[0]` → horizontal axial half-extent (`radiusX`)
  - `radius[1]` → vertical axial half-extent (`radiusY`)
  - `player.accessoryPoints` → starting accessory budget per player (default **2** when `player` omitted)
- Both radius components must be **≥ 0**. `accessoryPoints` must be **≥ 0**. Missing `map` / `radius`, wrong array length, non-integers, invalid JSON, or negative values fail fast.
- Documented defaults: radius **`[8, 6]`**, accessory points **`2`** (`CoreSettings.Defaults`).
- Host hooks: `RequireCoreMapRadius`, `RequireCoreAccessoryPoints`.

## Non-goals (for now)

- User settings file
- Configuring hex size, world seed, or spawn/faction parameters via core JSON
