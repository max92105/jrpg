---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11i-1 (Events mode and the event dialog, with dialogue steps)
Round: 1 (verdict: Ready to commit; build 0 warnings, 793 tests, format exit 0)

- open (Warning): MapEventComparison dialogue branch (Speech lines SequenceEqual) untested; no content map has a dialogue step, so ToEdit_NothingChanged_IsNull only covers swap/warp/giveItem; add ToEdit_UnchangedDialogue_IsNull
- open (Suggestion): ListOrder.Remove "one before" and "none left" branches untested; RemoveEventCommand_LastEvent_SelectsThePrevious / _OnlyEvent_SelectsNothing
- open (Suggestion): EventIds doc says deleting never hands an id on; plan risk line 448 says deleting the last event across dialog sessions frees it; align the comment
- open (Suggestion): SetObjectEventsEdit_Revert test sits in EventListViewModelTests; move to Maps/Editing/SetObjectEventsEditTests; add plan Done note listing extra types (EventViewModel, EventEditingViewModel, MapEventComparison, ListOrder, ObjectEventsWindow)
- verified: event id map-wide uniqueness ({uid}_{digits}, uids distinct, suffix has no '_', so no cross-object collision); OnlyWhen ComboBox values always in choices (validator rejects unknown object ids); right pane null DataContext safe (FallbackValue Collapsed); markers redraw on Map change and on open, EventMarks under IsHitTestVisible=False canvas; Events mode guarded in Delete key / drag / brush handlers
