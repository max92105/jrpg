---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice NB3 (Facing box in PlacedObjectEditing/PlacedObjectPanel,
TileImages.FieldSpriteFrameOf(file, facing), MapDrawing draws placed.Facing). 2026-11-12. Round 1.
Verdict: ready to commit. No Critical, no Warning.

- Suggestion (open): stale docs: PlacedObjectEditing.cs:98-100 Apply summary omits facing; :47 ObjectId summary says
  "uid, position, layer and events kept" though a swap to a tile object drops the facing; TileImages.cs:105
  "cached by the record" now record and facing.
- Suggestion (open): TileImages.Forget locals `sheetsOfFile`/`sheet` now hold (Sheet, Facing) keys; rename framesOfFile/key.
- Suggestion (open): PlacedObjectPanel.xaml:46 binds SelectedItem, the slice planned SelectedValue (free-levels rule);
  equivalent for a fixed enum list, but the Done note doesn't record it.
- NB2 suggestions (MapLayoutValidatorTests helper, NB2 test names): not carried, NB2 committed, suggestions only.
