---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/MO2a.md, slice MO2a (SelectionGlide on tabs and title; title pointer bob)
Round: 1 (verdict: Ready to commit)

- W1 SelectionGlide.Follow — the doc says "jumps on a wrap" but the code jumps on any move of more than one row; a ListCursor skipping a disabled row (MO2b lists) would jump instead of gliding; fix doc or make Follow wrap-aware before MO2b; open
- W2 SelectionGlideTests — no test that Follow(same row) mid-glide leaves the glide running (called every update; removing the guard freezes the highlight); open
- S1 TitleScene — row pitch EntryRowHeight + EntryGap now computed in two places; a RowPitch const; open
- S2 SelectionGlideTests — missing blank line before Follow_NeighbouringRow_Glides; open
