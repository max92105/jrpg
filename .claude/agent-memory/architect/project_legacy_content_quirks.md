---
name: legacy-content-quirks
description: Non-obvious formats and facts of legacy Assets JSON/art and legacy UI (maps, Back maps, tiles, objects, tilesheets, VFX, icons) that conversion and the MonoGame port must handle
metadata:
  type: project
---

Verified 2026-10-17/18 against the files:
- `Assets/Objects/Objects.json`: polygons as `"x,y"` strings, normalized 0..1 over tile bounds. `Width`/`Height` stale; tile groups not at (0,0). `IsNpc` false, `Meta` empty. Tile-group Z offsets are all 0 (an object draws on one layer). AbovePlayerZones are free-form concave polygons (canopies, roofs).
- `Assets/Tiles/Tileset.json`: PascalCase dict keyed by string id. All 283 tiles walkable and all from a tilesheet cell; Sprite/Frames/CollisionPolygon null.
- `Tilesheets.json`: configures only 2 (16 px FG_Forest_Fall, 64 px Floor) of ~10 referenced sheets; rest relied on a 48 px code default.
- Live maps: single-line camelCase, accidental `usedTileIds`/`abovePlayerZoneZ`, id-0 placeholder tiles, negative z, `zOverrides` empty everywhere.
- `Assets/Maps/Back/*`: older format, dropped (decision 39).
- Battle VFX are GIFs (AttackPlayer/AttackEnemy) - MonoGame can't load them. Legacy UI icons are XAML vector geometries in JRPG/UI/Styles/Art.xaml (no PNGs). `player.png` rows are Down, Left, Right, Up (new `Facing` enum order differs).
- Legacy battle and menus are mouse-driven; the intro cutscene is reachable only via F1.

**Why:** converter and render code must error on absent cases and handle present ones; the MonoGame port can't reuse WPF art paths.

**How to apply:** converter tests from real samples; check these before designing rendering. See [[migration-roadmap]].
