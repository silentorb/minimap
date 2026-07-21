# Agent notes — Minimap.Simulation.Types

## Purpose

This project **exclusively houses contracts** shared across Simulation, Extensive, App, and content extensions (dependency diamond).

## What may live here

- Interfaces and abstract bases (e.g. `AccessoryEffect`, `IShootEffect`)
- Definition / content bag types that Simulation and extensions both need (`AccessoryDefinition`, `CharacterDefinition`, `GameContent`, …)
- **Minimal boilerplate only**: constructors with invariant checks, property getters/setters, trivial abstract members such as `Clone()`

## What must not live here

- Sealed gameplay implementations (concrete accessory effects, systems, controllers)
- Tick / fire / spawn logic
- Anything that belongs in a content extension by default (accessory effects → CompuQuest or another extension)

Concrete effects and other game content types belong in extension libraries (default: **CompuQuest.Minimap**). Host Simulation may depend on contracts from this project; it must not embed extension-owned sealed effect classes.
