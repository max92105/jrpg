---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E12b1.md, E12b1 (new object rules + catalog step; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1513 tests)

- open (Warning): CreateObject's refusal while IdProblem/CategoryProblem stands is untested; a taken id would make SetObject silently replace the existing object. Add CreateObject_IdTaken_ReturnsFalseAndKeepsTheObject.
- open (Suggestion): MainViewModelTests ~line 672: old comment "Clicks the first cell..." now sits above ChooseTwoCellsNotTiles; move it back above ChooseCellNotATile.
- open (Suggestion): ObjectIdsTests InlineData "tree-4" doesn't match the name CapitalsOrSpaces; rename to ..._OtherCharacters_... or use "tree 4".
- follow-up: MainViewModel is 563 lines (was ~537); split before E12b2 adds more.
- follow-up: E12a re-cut stale SelectedCells can now reach CreateObject (cells past the grid become tiles).
