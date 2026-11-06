---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MS5 (MapNavigation: map list filter box, Ctrl+M via Find, Back to
previous map Alt+Left), round 1 (2026-11-05). Verdict: ready to commit.

- Open (Warning): no test reaches GoBackCommand.CanExecute == true, and RelayCommand.Execute skips CanExecute;
  deleting `Navigation.ShowEntries(entries)` in ShowMapList (empty window list, Back greyed) passes every test. Fix:
  assert CanExecute true before Execute in GoBack_MapLeftUnsaved_ShowsItWithItsChanges.
- Open (Suggestion): TryOpenFirstShown_TwoMatches_OpensTheFirst filters "o", which matches all three rows; use "e".
- Open (Follow-up, from MS4): SaveAll_SecondMapRefused puts the refused map last, so removing SaveAll's `break` passes.
- Fixed (from MS4): EditorTestContent empty contentRoot param; SaveAll_NewMapOpen now reads the written content.
