---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice FS5 (TileImages.FieldSpriteFrameOf crops the record's standing
cell, cache keyed by record; MapDrawing.DrawObject(placed) at AreaOver, standalone picture FittedSpriteArea;
FieldSpriteLayout deleted).
2026-11-12. Round 1. Verdict: ready to commit (no Critical, no Warning).

- Suggestion (open): MapDrawing.cs DrawObject(definition) summaries ("its tiles' first frames") and
  ObjectShapesWindow.xaml.cs:47 ("The object as the map draws it") don't say a tall NPC is shrunk into its cells.
- Suggestion (open): TileImages.cs:117-123 a frame past the edge reloads and decodes the PNG on every draw (bitmap not cached, only the mark is meant to be uncached).
- Suggestion (open): FittedSpriteArea ignores FeetPixels; picture's frame bottom on the box bottom, map's feet line on the cell bottom.
- FS4 Suggestion ObjectScreenAreaTests rounding: fixed (origin 10.6 now).
- FS3 Suggestion FieldSpriteLayout leftovers: fixed (type deleted).
- FS3 Suggestion AreaOver odd width: open (not this diff).
- FS4 Follow-up ObjectDefinition.cs "laid out like the player's": fixed in FS5.
