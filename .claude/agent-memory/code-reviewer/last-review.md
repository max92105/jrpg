---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/M3.md, slice M3 (battle panel shows resources)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical, open: PartyStatusView.cs:10 summary line starts with `//` not `///` -> CS1570, build fails.
- Suggestion, open: PartyStatusPanel ctor packs HP + resource rows into a tuple to stay under 4 params; a small named record (or the panel building its rows) reads clearer.
- Suggestion, open: PartyStatusPanel.Refresh re-reads AmountOf per resource that ResourceRowsText already read; could expose the amount from ResourceRowsText.
