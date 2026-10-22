---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E9a.md, E9a (object collision outline rules + CatalogDraft.SetObject; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1461 tests)

- open (Suggestion): ObjectBrowserViewModel.OnCatalogChanged branch where the chosen id is gone (undo of an added object -> SelectedObject null) and the ReferenceEquals short-circuit (tile-only change keeps the same Objects list) are untested.
- open (Suggestion): IsSameObject's `before with { lists = after's } == after` trick is non-obvious; one line in its comment saying it compares every non-list field would help.
