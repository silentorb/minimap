# Map layout

Single-screen hex arena shape and size. Technical: [hex-grid-shape.md](../../../technical/features/session/hex-grid-shape.md).

## Requirements

- The playable map is a **single-screen** hex arena (no scrolling camera for exploration).
- Map shape is a **rectangle in screen space** (pointy-top hex layout), not a circular axial disk and not an ellipse.
- Relative to the previous disk of axial radius **4**:
  - **Horizontal** half-extent: **2×** → axial **radiusX = 8**
  - **Vertical** half-extent: **1.5×** → axial **radiusY = 6**
- Default extents are configured in [`config/core.json`](../../../config/core.json) as `"radius": [8, 6]` (see [core settings](../../../technical/features/platform/core-settings.md)).
- Hex cells whose centers fall inside the axis-aligned rectangle are playable; outside are empty / boundary walls as today.

## Non-goals (for now)

- Multi-screen or streaming maps
- Non-rectangular irregular outlines
