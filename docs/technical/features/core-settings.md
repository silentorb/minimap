# Core settings

Shipped JSON configuration for developer-oriented (and user-editable) core game parameters. Loaded at the **Minimap.App** surface. Implements defaults for [map layout](../../game/features/map-layout.md) / [hex grid shape](hex-grid-shape.md).

## Requirements

- Core settings live in **`config/core.json`** (Godot path `res://config/core.json`).
- This file is distinct from a future **user settings** file (preferences that are not core bootstrap parameters).
- **Minimap.App** exposes load APIs: `CoreSettings.LoadFromJson` and `CoreSettings.LoadFromFile`. Simulation and Client do not perform file I/O for settings.
- `GameApp` loads core settings on ready via an exportable path (default `res://config/core.json`) and applies map radii when creating `GameSession`.
- **Vector convention:** paired numeric settings prefer a 2D vector. In JSON they are a length-2 numeric array `[n, n]`, deserialized to `SimVec2I`.
- Current schema:

```json
{
  "map": {
    "radius": [8, 6]
  }
}
```

  - `radius[0]` → horizontal axial half-extent (`radiusX`)
  - `radius[1]` → vertical axial half-extent (`radiusY`)
- Both radius components must be **≥ 0**. Missing `map` / `radius`, wrong array length, non-integers, invalid JSON, or negative values fail fast.
- Documented defaults: **`[8, 6]`** (`CoreSettings.Defaults`).

## Non-goals (for now)

- User settings file
- Configuring hex size, world seed, or spawn/faction parameters via core JSON
