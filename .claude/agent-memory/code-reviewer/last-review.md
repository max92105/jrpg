---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/T1.md, slice T1 (title shows only usable entries)
Round: 1 (verdict: Ready to commit)

- Suggestion: TitleSceneTests `Select` helper loops forever if the entry isn't listed (e.g. Load with no save); bound it by a press count and assert, so a wrong entry list fails instead of hanging — open
