# Agent notes — Minimap.Simulation.Navigation

## Purpose

Godot-backed navigation for Simulation: navmesh from floor hexes and crowd avoidance steering that implements Simulation `IMoveSteering`.

## What may live here

- Programmatic `NavigationRegion2D` / `NavigationAgent2D` construction (no Godot `[ScriptPath]` Node scripts)
- Implementations of Simulation navigation / steering interfaces
- Hex → `NavigationPolygon` baking

## What must not live here

- Presentation, HUD, or input (those stay in **Minimap.Client**)
- Authoritative movement integration (Simulation `ApplyMovement` / wall-slide)
- References to **Minimap.Client**
- AI brain / `AiController` (stays in **Minimap.Simulation**)

Depends on **Minimap.Simulation** + GodotSharp. **Minimap.App** constructs and parents hosts into the world scene.
