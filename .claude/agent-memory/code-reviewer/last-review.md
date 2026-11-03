---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice AI2 (AnimationImport, SetTileAnimations). Round 1.
Verdict: Ready to commit.

- open (Warning): AnimationImport.Apply picks MinBy(id) among sheets sharing the image, then refuses on its cut even
  when another sheet on that image has the .tsx's cut (SheetChoices allows several sheets per image); untested
- open (Suggestion): "Animated 1 tiles of x." for a single animated tile
- open (Follow-up): TiledTileset.Columns is set by the reader but read nowhere (AI1 code)
- fixed: all AI1 findings (duration/columns 0 now PositiveNumberOf, throws tested, "animated twice" refusal,
  empty animation message, map.md stray space)
