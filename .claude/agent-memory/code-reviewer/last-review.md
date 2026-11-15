---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-event-window-cleanup, slice EW2 (one Add step drop-down: DropDownChoice, StepKindMenu, DropDownMenu,
EditorDropDownStyles.xaml, EventViewModel.AddStepCommand). 2026-11-15. Round 1. Verdict: Fix the Criticals.

- Critical, open: Events/DropDownMenu.cs:17 sets the menu's DataContext locally; a right-click on Add step after
  switching events (ObjectEventsWindow, EventView DataContext = SelectedEvent) opens the old event's choices, adding the
  step to the hidden event. Probed (PowerShell -STA): local copy stays A after switch; PlacementTarget alone inherits
  and follows the switch. Fix: drop the line and its comment.
- Suggestion, open: AddStepChoices_Title_RunsNothing restates IsTitle => Command is null (test 1 already covers it).
- Suggestion, open: EditorDropDownStyles.xaml header names "Preset" (EW3's, not yet in the tree).
- Suggestion, open: the 6 px title margin also pads above the first title (TALK).

EW1 had no findings; nothing carried.
