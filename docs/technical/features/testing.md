# Automated testing (unit and functional)

This project separates **simulation logic** (`Minimap.Simulation`) from **Godot client code** (`Minimap.Client`). Automated tests mirror that split: unit tests stay fast and engine-agnostic, while Godot functional tests run against a real Godot process and control it remotely over protobuf gRPC.

For background on architecture and directories, see [Technical design](../technical-design.md).

## Frameworks and runners

- **xUnit + Microsoft.NET.Test.Sdk** for all test projects.
- **Godot functional automation** uses a runtime autoload node (`GodotRpcHost`) in `Minimap.Client` that hosts a gRPC server implementing contracts from `Minimap.Automation.Contracts`.
- Godot functional tests act as an RPC client and drive scene load, frame stepping, and input simulation from xUnit.

## Repository layout

| Area | Typical location | References | Purpose |
|------|------------------|------------|---------|
| **Unit** | `tests/unit/` (`Minimap.Simulation.Tests`) | `Minimap.Simulation` only | Grid math, generators, evolution rules, `GameWorld` APIs—no Godot runtime dependency. |
| **Functional (simulation)** | `tests/functional/Minimap.Functional.Tests` | `Minimap.Simulation` only | Broader simulation journeys (seeded world, movement, evolution loops). CI-friendly with `dotnet test` only. |
| **Functional (Godot client)** | `tests/functional/Minimap.Functional.Godot.Tests` | `Minimap.Simulation`, `Minimap.Client`, `Minimap.Automation.Contracts` | xUnit tests that launch Godot and control `WorldRoot` via protobuf gRPC RPC calls. |

See also [tests/functional/README.md](../../../tests/functional/README.md).

## Godot functional test flow

1. xUnit fixture starts `GODOT_BIN` with:
   - `MINIMAP_AUTOMATION_ENABLED=1`
   - `MINIMAP_AUTOMATION_HOST`
   - `MINIMAP_AUTOMATION_PORT`
2. Autoload `GodotRpcHost` starts gRPC server inside Godot.
3. Tests call RPC methods (`Ping`, `LoadMainScene`, `SimulateFrames`, `SetKeyState`, `GetWorldState`).
4. Fixture sends `Shutdown` and tears down process/channel.

## Commands

From repository root (after `dotnet restore`):

```bash
dotnet test tests/unit/Minimap.Simulation.Tests/Minimap.Simulation.Tests.csproj
dotnet test tests/functional/Minimap.Functional.Tests/Minimap.Functional.Tests.csproj
```

Godot client smoke (optional / machine with Godot):

```bash
export GODOT_BIN=/path/to/Godot_v4.x
dotnet test tests/functional/Minimap.Functional.Godot.Tests/Minimap.Functional.Godot.Tests.csproj
```

If `GODOT_BIN` is not set, run only unit + simulation functional suites.

## Related docs

| Topic | Document |
|-------|----------|
| Remote/headless Godot from dev container | [remote-headless-godot.md](remote-headless-godot.md) |
| Simulation vs client boundaries, `./tests` in tree | [Technical design](../technical-design.md) |
| Gameplay vision (not test mechanics) | [Game design](../../game/game-design.md) |
