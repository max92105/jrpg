---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-event-window-cleanup, slice EW3 (one Preset drop-down: EventListViewModel.PresetChoices/PresetMenu,
ApplyQuestGiverPreset(questId), ObjectEventsWindow Preset button). 2026-11-15. Round 1. Verdict: Ready to commit.

- EW2 Critical (DropDownMenu copying DataContext): fixed, DropDownMenu.Open sets only PlacementTarget/Placement/IsOpen.
- EW2 Suggestion (style header names Preset before EW3): fixed by EW3.
- EW2 Suggestions (title test restating IsTitle, title margin above first title): not carried (taste).
- Suggestion, open: the new Preset tooltip drops the old Quest giver button's "what it says once completed is a third
  event to add" hint.
