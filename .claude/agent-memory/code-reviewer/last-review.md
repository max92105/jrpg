---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice AI1 (TiledTilesetReader). Round 1.
Verdict: Ready to commit.

- open (Warning): TiledTilesetReader accepts duration="0" (TileAnimation of 0 ms, Refusal null; FrameAt divides by
  zero) and columns="0" (DivideByZeroException at id % columns, not ContentValidationException)
- open (Warning): the missing <image source>, non-<tileset> root and frame-without-tileid/duration throws are untested
- open (Suggestion): duplicate animated <tile id> -> Dictionary.Add ArgumentException, not ContentValidationException
- open (Suggestion): empty <animation/> refusal says "one frame is a still tile" for 0 frames
- open (Suggestion): docs/map.md Art row has a stray space before the comma: "`PngSize` , `TiledTilesetReader`"
- fixed: AN2's two Suggestions (TileAtlasTests First(), "the water moves" comment), grep finds neither
- dropped: CO4a StartNew column-clip Follow-up, listed in docs/plans/backlog.md
