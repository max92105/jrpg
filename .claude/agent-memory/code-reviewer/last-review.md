---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/S3b.md, slice S3b (choice screen information area: portrait, texts, stats, fade)
Round: 1 (verdict: Ready to commit)

- Warning: LeaderChoiceScene.ShowSelectedProfile (Show on selection change) has no test; scene tests read SelectedProfile from the menu, not what the view draws. Add an internal ShownIndex on the view (or scene) and a test Update_Down_ShowsTheNextProfile — open
- Suggestion: LeaderProfileView.Update/Draw lack the one-line doc comments its Show has — open
- Follow-up: S3a's FooterBar.TextTop finding (4 copies of footer centering) — still open from S3a
