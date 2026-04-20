# Functional tests

Simulation-focused flows live in **`Minimap.Functional.Tests`** (`Minimap.Simulation` only). They run under plain `dotnet test` with no Godot executable.

Godot client smoke tests live in **`Minimap.Functional.Godot.Tests`**. These tests are xUnit-based and drive Godot through the runtime autoload RPC host (`GodotRpcHost`) using protobuf gRPC calls.

## Godot functional prerequisites

- Set **`GODOT_BIN`** to a Godot 4.x executable that this environment can run.
- Optional: set **`MINIMAP_AUTOMATION_PORT`** to force a fixed gRPC port (otherwise tests auto-pick a free local port).
- Optional: set **`GODOT_REMOTE_URL`** if you want launcher health checks before test execution.

From repo root:

```bash
export GODOT_BIN=/path/to/Godot_devtools   # WSL example: /mnt/c/Apps/Godot/Godot_v4.6-stable_win64.exe
./scripts/run_godot_functional_tests.sh
```

Or invoke directly:

```bash
export GODOT_BIN=/path/to/Godot_devtools
dotnet test tests/functional/Minimap.Functional.Godot.Tests/Minimap.Functional.Godot.Tests.csproj
```

Run everything that does **not** need Godot locally or in CI:

```bash
dotnet test tests/unit/Minimap.Simulation.Tests/Minimap.Simulation.Tests.csproj
dotnet test tests/functional/Minimap.Functional.Tests/Minimap.Functional.Tests.csproj
```

Remote launcher + dev container defaults: [docs/technical/features/remote-headless-godot.md](../../docs/technical/features/remote-headless-godot.md).
