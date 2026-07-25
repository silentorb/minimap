# Automated testing (unit and functional)

This project separates **simulation logic** (`Minimap.Simulation`) from **Godot client code** (`Minimap.Client`). Automated tests mirror that split: unit tests stay fast and engine-agnostic, while Godot functional tests run against a real Godot process and control it remotely over protobuf gRPC.

For background on architecture and directories, see [Technical design](../technical-design.md).

## Frameworks and runners

- **xUnit + Microsoft.NET.Test.Sdk** for all test projects.
- **Godot functional automation** uses a runtime autoload node (`GodotRpcHost`) in `Minimap.Client` that hosts a gRPC server implementing contracts from `Minimap.Automation.Contracts`.
- Godot functional tests act as an RPC client. They load **playbook libraries** (managed DLLs) into the live process and run named **playbooks** in-process; the gRPC surface stays a thin control plane.

## Godot as subject-under-test

The Godot process is a **minimally modified** SUT, not a per-test custom build.

| Allowed | Not allowed |
|---------|-------------|
| **General** automation framework code in the normal game process (RPC host, playbook loader/registry, shared helpers) — dormant unless enabled (e.g. `MINIMAP_AUTOMATION_ENABLED`) | Building or configuring a **different** Godot process / project / scene set **per test case** |
| One shared headless launch of the **same** game binary/project for many tests | Baking test-case scenes, test runners, or case-specific scripts into the shipped Godot project as the primary model |
| After start, remotely commanding the process to **load specialized playbook libraries** and run named playbooks | Requiring GdUnit/Gut-style in-project test trees as the main approach |

Specialization lives in externally built playbook DLLs under `tests/`; the process under test stays one general automation-capable build.

## Repository layout

| Area | Typical location | References | Purpose |
|------|------------------|------------|---------|
| **Unit** | `tests/unit/` (`Minimap.Simulation.Tests`, `Minimap.App.Tests`) | Simulation-only or App (settings load) | Grid math, generators, `GameWorld` APIs, core settings JSON—no Godot runtime dependency. |
| **Functional (simulation)** | `tests/functional/Minimap.Functional.Tests` | `Minimap.Simulation` only | Broader simulation journeys (seeded world, movement, combat). CI-friendly with `dotnet test` only. |
| **Functional (Godot playbooks)** | `tests/functional/Minimap.Functional.Godot.Playbooks` | Contracts + `Minimap.Automation` | One or more `IPlaybook` types per library; loaded into Godot after start. |
| **Functional (Godot client)** | `tests/functional/Minimap.Functional.Godot.Tests` | Contracts (gRPC client) | xUnit tests that launch Godot, `LoadPlaybookLibrary`, and `RunPlaybook`. |
| **Automation helpers** | `src/Minimap.Automation` | GodotSharp only | Standalone in-process helpers (frame wait, movement keys, scene lookup). No Contracts/playbook/test references. |
| **Automation contracts** | `src/Minimap.Automation.Contracts` | Protobuf/gRPC | Wire protocol + `IPlaybook` / `IPlaybookContext` / `PlaybookResult`. |

See also [tests/functional/README.md](../../../tests/functional/README.md).

## Godot functional test flow

1. xUnit fixture starts `GODOT_BIN` with:
   - `MINIMAP_AUTOMATION_ENABLED=1`
   - `MINIMAP_AUTOMATION_HOST`
   - `MINIMAP_AUTOMATION_PORT`
2. Autoload `GodotRpcHost` starts gRPC server inside Godot.
3. Fixture calls `LoadPlaybookLibrary` with the path to a playbook DLL (multiple libraries supported; ids are `AssemblyName.PlaybookId`).
4. Each test calls `RunPlaybook` — assertions and scene/input orchestration run **inside** Godot in the playbook.
5. Fixture sends `Shutdown` and tears down process/channel.

Legacy RPCs (`LoadMainScene`, `SimulateFrames`, `SetKeyState`, `GetWorldState`) remain for compatibility while playbooks become the primary path.

## Commands

From repository root (after `dotnet restore`):

```bash
dotnet test tests/unit/Minimap.Simulation.Tests/Minimap.Simulation.Tests.csproj
dotnet test tests/unit/Minimap.App.Tests/Minimap.App.Tests.csproj
dotnet test tests/functional/Minimap.Functional.Tests/Minimap.Functional.Tests.csproj
```

Godot client smoke (dev container sets `GODOT_BIN` automatically):

```bash
./scripts/run_godot_functional_tests.sh
```

Or equivalently (after building Automation + Playbooks + `minimap.csproj`):

```bash
dotnet test tests/functional/Minimap.Functional.Godot.Tests/Minimap.Functional.Godot.Tests.csproj
```

Outside the container, export `GODOT_BIN` to a Godot 4.6 .NET executable first. If `GODOT_BIN` is not set, run only unit + simulation functional suites.

## Bug regressions / debugging

Debugging should be as test-driven as practical. When a **user-reported bug** was not caught by the suite, a regression test is part of the fix—unless a sound test is not available.

### Workflow

1. Reproduce the failure (manually or via a new failing test).
2. Prefer a **failing test first** at the **lowest layer** that can express the bug.
3. Fix product code; keep the test; leave it in the suite.
4. Assert **documented** requirements (feature docs / [technical design](../technical-design.md)). If the bug reveals missing documented behavior, update the docs in the same change and test against that—not against undocumented quirks.

### Layer choice

| Prefer | When |
|--------|------|
| **Unit** (`tests/unit/`) | Pure logic, settings load, path resolution, state machines—no Godot runtime. |
| **Simulation functional** | Multi-step simulation journeys still without Godot. |
| **Godot playbook** | Scene lifecycle, input routing, lobby/world UI, or other client-only behavior. |

### Escalate instead of a bad test

Stop and discuss with the user (do **not** quietly ship weak coverage) when a reproduction would require:

- Hacking or expanding general testing harnesses beyond the fix
- Likely **brittle** assertions (e.g. parsing project XML, depending on ambient build artifacts)
- Likely **flaky**, **hanging**, or **non-deterministic** behavior
- Deliberately breaking the SUT environment in ways happy-path automation does not support cleanly

Then the user can choose: skip the test for this bug, invest in harness work, or redesign code for testability.

Agent rule: [`.cursor/rules/bug-regression-tests.mdc`](../../../.cursor/rules/bug-regression-tests.mdc).

## Related docs

| Topic | Document |
|-------|----------|
| Simulation vs client boundaries, `./tests` in tree | [Technical design](../technical-design.md) |
| Gameplay vision (not test mechanics) | [Game design](../../game/game-design.md) |
