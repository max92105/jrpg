---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/M2b.md, slice M2b (MemberColumnView drawn on member pages)
Round: 1 (verdict: Ready to commit)

- S1 GameMenuScene.DrawPage picks the column layout by `page is IMemberPage` while the sidebar draws on ShowsPartySidebar; a member page returning true would overlap both; say in IMemberPage's doc that member pages never show the sidebar (or derive it); open
- S2 StatusPage.ShowsPartySidebar flip (decision 266) has no test, unlike Equipment's; open
- F1 IMenuPage.ShowsPartySidebar and the sidebar's shown-member marking: M2c cleanup; follow-up
