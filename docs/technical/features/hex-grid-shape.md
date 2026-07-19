# Hex grid shape

Elliptical playable hex map. Implements [../../game/features/map-layout.md](../../game/features/map-layout.md).

## Requirements

- `HexGrid(int radiusX, int radiusY)` builds the playable set.
- Optional convenience: `HexGrid(int radius)` → `HexGrid(radius, radius)` for square/disk-like tests.
- A hex `h` is included when \((x / maxX)^2 + (y / maxY)^2 \le 1\), where `(x, y) = HexWorldLayout.ToWorld(h)` and:
  - `maxX` = world **X** of `HexAxial(radiusX, 0)` (pure horizontal extreme)
  - `maxY` = absolute world **Y** of `HexAxial(0, radiusY)` (vertical semi-axis length `1.5 * hexSize * radiusY`)
- Note: on a pointy-top layout, `HexAxial(0, radiusY)` also has a nonzero X, so that cell itself may fall **outside** the axis-aligned ellipse; vertical extremes are hexes near X≈0 with `|R|` near `radiusY` (e.g. `(radiusY/2, -radiusY)` when even).
- Enumerate candidates in an axial bounding box large enough to cover the ellipse, then filter.
- Expose `RadiusX` / `RadiusY`.
- Default runtime extents: **radiusX = 8**, **radiusY = 6**, loaded from core settings (`config/core.json` `"radius": [8, 6]`; see [core-settings.md](core-settings.md)).
- Wall / out-of-map boundary collider generation continues to treat neighbors outside `Contains` as solid boundary hexes.
