# Refugee Road - agent notes

A standalone game built on the **Crossroads engine** (pulled as a git UPM package). This file covers what's
specific to this game; the full build/content/press/release playbook lives in the engine repo:
**`orbenozio/Crossroads` -> `CLAUDE.md`** (https://github.com/orbenozio/Crossroads/blob/master/CLAUDE.md).
Read that for the recipe and the gotchas (Unity won't re-resolve a manifest live -> restart; use batchmode
`-runTests` not the bridge's; move files with their `.meta`; etc.).

## This game

- **Format:** **journey / map** (the same `EventEngine`, wrapped in a node map with a goal to reach). This is
  the reference for the journey format - it adds `map.json` and drives the engine via `MapGraph` +
  `engine.EnterNode(id)` instead of `Deck`.
- **Map:** `border -> checkpoint -> forest -> {river|town} -> haven` (goal = `haven`). 6 story nodes.
- **Identity:** productName `Refugee Road`, app id `com.crossroads.refugeeroad`.
- **Engine pin:** `com.orbenozio.crossroads` at `#v0.1.1` (see `Packages/manifest.json`).

## Layout

- `Assets/Game/` - `GameBootstrap.cs`, `Crossroads.Game.RefugeeRoad.asmdef` (-> Engine + UI),
  `Content/{story.json,map.json,resources.asset,theme.asset}`, `Scenes/Game.unity`.
- `Assets/Game/Tests/` - the full border->haven journey-to-victory test + architecture test
  (local `Assets/Game/Content/...` paths; validates story AND map via `MapValidator`).
- `Assets/Editor/` - `build_webgl.cs` / `build_android.cs` (scene `Assets/Game/Scenes/Game.unity`).
- `press/` (tracked) when there's marketing media. `itch/` (gitignored) for the WebGL build zip.

## Common tasks

- **Test:** `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults out.xml -logFile log`
  then read `out.xml`.
- **Build:** the `build_webgl` / `build_android` bridge tools (block; also write `Builds/last_build.json`).
- **Bump engine:** re-pin `Packages/manifest.json` to the new tag, open the editor once to re-resolve, commit
  the updated `packages-lock.json`.
