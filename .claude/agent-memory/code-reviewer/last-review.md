---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/M3b.md, slice M3b (Status page look: caps labels, large values, hairlines)
Round: 1 (verdict: Ready to commit)

- S1 Stats+Speed merged in three places (view _parameterLabels, ShownSheet stats, StatusPage.ShownLabels); one "parameters" array in StatusLabels/StatusSheet would join them; open
- S2 DrawParameters' switch hardcodes index 3 as speed; a fifth parameter lands at (3,1) off the grid; open
- S3 StatusLabels summary "its captions in caps once here;" reads awkwardly; open
- F1 EquipmentSheetView keeps the old StatLineHeight/ValueOffset row look, now unlike the Status page's rows; follow-up
