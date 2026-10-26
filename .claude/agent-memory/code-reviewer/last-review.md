---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/M2a.md, slice M2a (MemberColumn model: members, shown, scroll)
Round: 1 (verdict: Ready to commit)

- W1 MemberColumn.KeepShownInView is the third copy of the keep-in-view scroll rule (ListCursor:84-88, PartySidebar:129-135); propose one shared Ui type/function; open
- S1 Refresh_ShownMember_IsMarked asserts Same(page.Shown, Members[ShownIndex]); pin ShownIndex == 1 after one Right; open
