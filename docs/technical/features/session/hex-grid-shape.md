# Hex grid shape

Rectangular playable hex map. Implements [map-layout.md](../../../game/features/session/map-layout.md).

## Requirements

- `HexGrid(int radiusX, int radiusY)` builds the playable set.
- Optional convenience: `HexGrid(int radius)` → `HexGrid(radius, radius)` for square tests.
- A hex `h` is included when `|x| ≤ maxX` and `|y| ≤ maxY`, where `(x, y) = HexWorldLayout.ToWorld(h)` and:
  - `maxX` = world **X** of `HexAxial(radiusX, 0)` (pure horizontal extreme)
  - `maxY` = absolute world **Y** of `HexAxial(0, radiusY)` (vertical half-extent `1.5 * hexSize * radiusY`)
- Note: on a pointy-top layout, `HexAxial(0, radiusY)` also has a nonzero X, so that cell itself may fall **outside** the axis-aligned rectangle; vertical extremes are hexes near X≈0 with `|R|` near `radiusY` (e.g. `(radiusY/2, -radiusY)` when even). Screen-corner cells (near `|x|≈maxX` and `|y|≈maxY`) are included.
- Enumerate candidates in an axial bounding box large enough to cover the rectangle, then filter.
- Expose `RadiusX` / `RadiusY`.
- Default runtime extents: **radiusX = 8**, **radiusY = 6**, loaded from core settings (`config/core.json` `"radius": [8, 6]`; see [core-settings.md](../platform/core-settings.md)).
- Wall / out-of-map boundary collider generation continues to treat neighbors outside `Contains` as solid boundary hexes.

## Client presentation

- Project display: base viewport **1280×720**, `window/stretch/mode=canvas_items`, `window/stretch/aspect=expand` so UI fills the OS window without letterboxing.
- `WorldView` recenters `Camera2D` on the map hex average; it does **not** dynamically zoom to fit or cover the viewport (hex world scale stays fixed).
- Editor Play (Godot 4.4+): floating **embedded** Game windows default to **Fixed Size**, which keeps a centered 1280×720 panel and ignores project stretch. This repo enables **Stretch to Fit** via [`addons/minimap_editor_defaults`](../../../addons/minimap_editor_defaults/) so Play respects stretch settings. If Play still shows a fixed centered panel, set the Game bar sizing to **Stretch to Fit** (or disable Embed Game on Next Play).
