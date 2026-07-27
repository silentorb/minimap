# Extension libraries

Built extension DLLs (e.g. `CompuQuest.Minimap.dll`) are copied here on build for local load via `config/extensions.json`.

Each extension also ships a content directory named after the assembly (e.g. `CompuQuest.Minimap/accessories/`, `CompuQuest.Minimap/actors/`, `CompuQuest.Minimap/domains/`). On every CompuQuest (or Godot host) build, that directory is **mirrored** from the extension’s `config/` tree: the destination is wiped, then all `*.json` files are copied. Retired definitions therefore disappear instead of stacking beside replacements.

Binary outputs (`*.dll`, `*.pdb`) and copied content directories are **gitignored**. They **survive `git pull`** on a second clone (for example a Windows checkout used only to run Godot). After a pull, a normal Godot build/Play is enough to refresh this tree via the host’s project reference to CompuQuest. If a retired lobby ability ever still appears, delete `extensions/<AssemblyName>/` and rebuild (escape hatch).

Author JSON under `src/CompuQuest.Minimap/config/`.
