---
name: legacy-content-quirks
description: Non-obvious formats and facts of legacy Assets JSON (maps, Back maps, tiles, objects, tilesheets) that the content converter must handle
metadata:
  type: project
---

Verified 2026-10-17 against the files:
- `Assets/Objects/Objects.json`: polygons as `"x,y"` strings, normalized 0..1 over tile bounds. `Width`/`Height` stale (tree says 1x1, spans 4x4); tile groups not at (0,0). `IsNpc` false, `Meta` empty everywhere.
- `Assets/Tiles/Tileset.json`: PascalCase dict keyed by string id. All 283 tiles walkable; Sprite/Frames/CollisionPolygon null everywhere.
- `Tilesheets.json`: configures only 2 of 9 sheets used (rest relied on a 48 px code default).
- Live maps: single-line camelCase with accidental `usedTileIds`/`abovePlayerZoneZ`, id-0 placeholder tiles, negative z. All objects have uids; no single `event`. Tile triggers only Warp; object events GiveItem/SwapObject/Warp.
- `Assets/Maps/Back/*`: OLDER format (tiles without z, objects `{id,x,y,zIndex,hasCollision}` without uid/events, Test.json embeds `objectDefinitions`). Back/start.json has the only Dialogue trigger; `Custom Map.json` is an older copy of Back/start.json. No Battle or Item triggers/actions exist anywhere.
- The intro cutscene is C# reachable only via the F1 test key.

**Why:** the part 2 converter (ADR 0005) must error on absent cases and convert present ones.

**How to apply:** require converter tests built from real samples (linked `legacy-assets/`). See [[migration-roadmap]].
