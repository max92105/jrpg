---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E3a.md, E3a (CatalogDraft: one live catalog in the editor, refactor)
Round: 1 (verdict: Ready to commit; verify all pass, 1320 tests)

- open (Suggestion): EventEditingViewModel.cs:13 blank line left after the class brace where the two fields were removed.
- open (Suggestion): no test pins "reads Current at each use" (e.g. ObjectPlacing.ObjectAt finds an object added to the draft after construction); worth adding with the first change method (E3d/E9).
- open (Follow-up): TileImages caches cropped tiles by id forever; E3d/E5b changing a tile's cell or sheet will draw the stale image unless the cache is invalidated on Current change.
- open (Follow-up): MainWindow partials still read `_viewModel.Catalog!` (three spots); fine now, revisit if Content becomes the only entry point.
