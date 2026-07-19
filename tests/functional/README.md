# Functional tests

Simulation-focused flows live in **`Minimap.Functional.Tests`** (`Minimap.Simulation` only). They run under plain `dotnet test` with no Godot executable.

Godot client smoke tests live in **`Minimap.Functional.Godot.Tests`**. These tests are xUnit-based and drive Godot through the runtime autoload RPC host (`GodotRpcHost`) using protobuf gRPC calls.

## Godot functional prerequisites

- Set **`GODOT_BIN`** to a Godot 4.x .NET executable that this environment can run. The **dev container** sets this automatically to the installed Linux binary.
- Optional: set **`MINIMAP_AUTOMATION_PORT`** to force a fixed gRPC port (otherwise tests auto-pick a free local port).

From repo root (in the dev container, `GODOT_BIN` is already set):

```bash
./scripts/run_godot_functional_tests.sh
```

Or invoke directly:

```bash
dotnet test tests/functional/Minimap.Functional.Godot.Tests/Minimap.Functional.Godot.Tests.csproj
```

Run everything that does **not** need Godot locally or in CI:

```bash
dotnet test tests/unit/Minimap.Simulation.Tests/Minimap.Simulation.Tests.csproj
dotnet test tests/functional/Minimap.Functional.Tests/Minimap.Functional.Tests.csproj
```

See [docs/technical/features/testing.md](../../docs/technical/features/testing.md) for the full testing overview.
