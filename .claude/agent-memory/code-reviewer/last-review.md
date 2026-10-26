---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/M2c.md, slice M2c (party sidebar's shown-member marking removed)
Round: 1 (verdict: Ready to commit)

- W1 PartySidebar.cs:7 summary still says "Refreshed every update a page is open"; now only while a page that shows it is open (GameMenuScene.RefreshMemberViews); open
- S1 GameMenuScene.ShowsSidebar could add `is not IMemberPage` so a member page overriding the DIM can't show both columns; open
