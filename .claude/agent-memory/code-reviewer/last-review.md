---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/S1b.md, slice S1b (Choosing asks "Begin as X?" first)
Round: 1 (verdict: Ready to commit)

- Suggestion: LeaderChoiceScene keeps _members only to name the chosen one; _names already holds the names (open)
- Suggestion: the open-prompt/draw-if-asking/close-on-answer shape now appears in SavePage, SystemPage, LeaderChoiceScene; a fourth copy should get a shared holder (open)
- Follow-up: StartMapScenarios.StartWithWholeParty hardcodes "kamilia"; C2 must drop her there (open, carried from L1, outside slice).
