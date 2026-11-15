---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-event-window-cleanup, slice EW5 (conditions folded in an Expander under
EventViewModel.ConditionSummary). 2026-11-15. Round 1. Verdict: Ready to commit.

- EW4 Critical (EventView ScrollViewer horizontal Auto): fixed (now Disabled, EventView.xaml:99).
- Suggestion, open: the header's tooltip is fixed text, so a trimmed summary (narrow pane) can't be read whole;
  could show ConditionSummary too.
- Follow-up, open: EW3's Preset tooltip (ObjectEventsWindow.xaml:25) still lacks the quest giver's "third event to
  add" hint; not in backlog.md.
