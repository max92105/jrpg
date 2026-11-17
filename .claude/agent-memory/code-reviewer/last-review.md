---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WS3b (shell wiring: MainViewModel.DatabaseWorkspace, ShowDatabase guard + Open,
ShowWorld through TryClose, GoBackToMap stopping; Refusal line in DatabaseWorkspaceView.xaml).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion: GoBackToMapCommand summary "World, then the previous map" doesn't say it stops when the page refuses — open
- Suggestion: Refusal TextBlock without TextWrapping in a centered StackPanel clips a long problem — open
- WS3 Suggestion NotesEditing.AppliesCount -> ApplyCount — fixed
- Known gap (not a finding): CatalogDialog changing the draft while Database shown restarts the session; WS4's scope
