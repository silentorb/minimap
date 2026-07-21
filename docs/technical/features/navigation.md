# Navigation (pathfinding / crowd)

Simulation-owned movement steering for AI floor goals. Implements game [ai.md](../../game/features/ai.md). Related: [controllers.md](controllers.md).

## Requirements

- **Interfaces live in Minimap.Simulation** (`IMoveSteering`, and any map-bake ports the implementation needs). Simulation itself stays **Godot-free**.
- **Headless default:** `DirectMoveSteering` — move intent is the normalized vector from pawn position toward the goal (or zero when paused / arrived).
- **Godot play:** **`Minimap.Simulation.Navigation`** may reference Godot. It builds a **`NavigationRegion2D`** navmesh from floor hexes and provides **`GodotCrowdSteering`** (`NavigationAgent2D` with avoidance) that implements `IMoveSteering`.
- **Minimap.App** constructs the navigation host, parents it under the world scene, **rebuilds** the mesh on session bind, level regeneration, and world evolution, and **upgrades** each `AiController` from Direct to Godot crowd steering (including mid-wave spawns).
- Steering only writes **`MoveIntent`**; Simulation `ApplyMovement` + wall-slide remain authoritative for positions.
- **Minimap.Client** does **not** own navigation (presentation / input only).

## Non-goals (for now)

- Player pathfinding
- Cover / tactical query APIs beyond wander goals
