---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, S1-2 (Saving from the menu: ListCursor window, SaveSlotList/View, SavePage, SystemPage Save entry, GamePaths, Program wiring)
Round: 1 (verdict: Ready to commit; build 0 warnings, 976 tests pass on rerun after a known flaky Game host crash, format exit 0)

- open (Warning): SavePage implements IMenuPage but is only ever used as a concrete SavePage inside SystemPage; TitleMessageId has no reader and LeavesOnCancel is constant true, so SystemPage's `&& _savePage.LeavesOnCancel` is dead until S3
- open (Suggestion): SavePage.Update throws when not opened while Draw silently returns; pick one contract (or note why)
- open (Suggestion): ListCursor.VisibleRows public getter has no reader (SaveSlotListView uses SaveSlotList.VisibleRows)
- open (Suggestion): party separator "   " and the date format are hardcoded in SaveSlotListView; could live in the save_member/save_details messages
- accepted (plan/hand-off): write IOException not handled until S3; 8 rows instead of 5; names+levels instead of portraits
- follow-up: FontLibrary non-font test crashes the Game test host intermittently (again this review)
