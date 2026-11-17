---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WS3 (headless part: TryApplyShown, ShowPage, TryClose, Refusal; decisions 919, 939,
1008; the shell wiring split to WS3b).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion: NotesEditing.AppliesCount reads oddly; ApplyCount — open
- WS2 Suggestion class summary "any change of the draft" — fixed (now "any new catalog in the draft")
- WS2 Follow-up ShowDatabase guard while Database shown — moved to WS3b's "Note from WS2 review" and its test list
  (ShowDatabase_DatabaseShown_KeepsWhatWasTyped); its record is WS3b, not carried
