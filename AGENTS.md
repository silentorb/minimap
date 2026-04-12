# Agent notes — marloth

## Project

- **Engine**: Godot **4.6**, Forward Plus renderer, **Jolt** for 3D physics.
- **Entry**: `run/main_scene` is `res://main.tscn` (see `project.godot`).
- **Name / assembly**: Application id is `marloth`; `project.godot` sets `[dotnet]` `project/assembly_name` for C# when used.

## Layout

- Primary game content lives at the repo root (`project.godot`, scenes, scripts, assets).
- **`.mnt/unreal/marloth/`** is a **read-only** bind of external Unreal source (see `.devcontainer/devcontainer.json`). Treat it as reference or legacy context unless the task explicitly concerns it; do not assume it is built or edited as part of this Godot tree.

## Conventions

- Prefer changing game logic and scenes in this repo; keep Godot editor–managed files (`*.tscn`, `project.godot`) consistent with how Godot serializes them.
- Match existing script language and style in the files you touch (GDScript vs C#).

## Environment

- Godot is not in the current dev container environment and exists outside of it in Windows.
