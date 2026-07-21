# Extension libraries

Built extension DLLs (e.g. `CompuQuest.Minimap.dll`) are copied here on build for local load via `config/extensions.json`.

Each extension may also ship a content directory named after the assembly (e.g. `CompuQuest.Minimap/accessories/`, `CompuQuest.Minimap/characters/`), copied from that project’s `config/` tree on build. App loads those JSON definitions after the DLL registers.

Binary outputs (`*.dll`, `*.pdb`) and copied content directories are gitignored. Building the Godot host (`minimap.csproj`), `CompuQuest.Minimap`, or the solution copies them here. Author JSON under `src/CompuQuest.Minimap/config/`.
