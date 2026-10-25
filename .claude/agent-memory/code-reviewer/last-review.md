---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/MP2.md, slice MP2 (no screen shows MP)
Round: 1 (verdict: Ready to commit)

- Suggestion, open: VitalsText.cs keeps two blank lines after `MaxHp` where the MP properties were.
- Suggestion, open: the `(column, line)` placement (HP alone, then pairs) is copied in StatusPage.DrawParameters and EquipmentSheetView.DrawParameters; share one function if a third page appears.
- Follow-up, open: docs/map.md line 36 still says "stats and HP/MP" (Vitals keeps MP until MP3b).
