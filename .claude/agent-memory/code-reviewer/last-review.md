---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-event-window-cleanup, slice EW4 (XAML only: step list beside the step's fields in EventView, warp row
split, window sizes and minimums). 2026-11-15. Round 1. Verdict: Fix the Criticals, then commit.

- Critical, open: EventView.xaml:89 ScrollViewer HorizontalScrollBarVisibility="Auto" measures EventStepEditor at
  infinite width, so the Wrap boxes (dialogue lines TextBox, MovePathText, Problem) never wrap; fix with "Disabled"
  (every fixed row fits ~428 px) and reword the comment.
- EW3 Suggestion (Preset tooltip drops the quest giver's "third event to add" hint): still missing, carried as a
  Follow-up.
