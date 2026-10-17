---
name: legacy-content-quirks
description: Non-obvious formats of legacy Assets JSON (maps, tiles, objects) that new Jrpg.Data readers must handle
metadata:
  type: project
---

Legacy content formats are mixed:
- `Assets/Objects/Objects.json` stores polygons as `"x,y"` strings (Newtonsoft serializing WPF `Point`).
- `Assets/Tiles/Tileset.json` is PascalCase, keyed by the string tile id.
- `Tilesheets.json` is PascalCase.
- Maps (`Assets/Maps/*.json`) are single-line camelCase System.Text.Json with polymorphic `"trigger":{"type":...}` on tiles and `"events":[...]` on objects.
- Old maps may carry a single `"event"` and objects without a `uid` (the legacy loader derived `"{id}@{x},{y}"`).
- Tile and object ids are ints, while content rules want string ids.

**Why:** the part 2 map loader and its ADR (read legacy as-is vs one-time conversion) depend on these.

**How to apply:** when planning map loading, check these against the current files and require loader tests that use real legacy samples. See [[migration-roadmap]].
