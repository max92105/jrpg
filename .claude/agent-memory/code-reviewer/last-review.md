---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/P1a.md, slice P1a (one pointer and one selection look; decisions 252, 253)
Round: 1 (verdict: Ready to commit)

- W1 Equipment page: sheet pointer plus PartySidebarView's always-on shown-member pointer = two diamonds while the sheet has the cursor; open
- S1 CommandPanelView.DrawRowBacks draws CommandRowFill in both branches; hoist above the if; open
- S2 Party cards keep a full-bright band while the action choice has the cursor (sheet/System/Inventory dim theirs); open
