---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/SZ1.md, slice SZ1 (menu sizing pass, decision 261)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: UiTheme.cs:63 says rows are 56 px (now 64) and :66 says the inset is 44 px (now 48); SquareShrink.cs:6 says icons are 26 or 40 px (now 24, 32, 40); open
- Warning: StatusPage.cs:51 VitalBarTop 26 unchanged while the HP line's body text went 18->26 px; the bar starts on the Lato baseline (~25.7 px) and touches the text; set it to about 34; open
- Suggestion: EquipmentSheetView.cs:129 comment "Max HP alone on the first line" now that the label says HP; open
