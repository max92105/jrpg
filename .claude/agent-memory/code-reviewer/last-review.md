---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E4a.md, E4a (Mark blocked)
Round: 1 (verdict: Ready to commit; verify all pass, 1358 tests)

- open (Suggestion): TileBrowserViewModel.OnCatalogChanged reacts to every CatalogDraft property, so the now-observable IsDirty rebuilds SheetTiles and redraws the sheet marks on first edit and on each save; filter on nameof(CatalogDraft.Current).
- open (Suggestion): ConfirmClose with map + tiles dirty ("Map 'x' and the tiles have...") has no test; only the tiles-only text is pinned.
- open (Suggestion): ConfirmClose repeats ConfirmLeaveCurrentMap's EndStroke + "Discard them?" wording; one private method building the "what's unsaved" phrase would keep both texts in step.
- open (Follow-up): MainViewModel.cs at 367 lines (was ~337 before the slice); split dirty/confirm questions or map-list handling out.
