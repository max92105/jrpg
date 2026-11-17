---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WS4 (Undo, Redo, Save, Save all, close and the catalog dialogs apply the shown
Database page first; UnappliedPage, decision 1009). 2026-11-17. Round 1. Verdict: Ready to commit (no Critical).

- Warning: CombinedHistory.TryApplyShownPage copies the page refusal into History.Refusal (status bar,
  MainWindow.xaml:166), cleared only by RecordStep/Undo/Redo: fixing the problem or Revert leaves "has a problem... Fix
  it or press Revert" in the status bar; same for SaveError's "Not saved: ..." — open
- Suggestion: UnappliedPage is a string? line, reads as a CatalogPage; UnappliedPageEntry/Line — open
- WS3b Suggestion GoBackToMapCommand summary names the refusal — fixed
- WS3b Suggestion Refusal TextBlock wraps (TextWrapping, MaxWidth 640) — fixed
