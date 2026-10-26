---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/W1a.md, slice W1a (untitled framed window, battle action name)
Round: 1 (verdict: Ready to commit)

- S1 BattleBannerView.Measure re-derives the frame height from FramedWindowLayout.PaddingY to turn a top into a center: give the layout a top-anchored factory or keep the arithmetic in the layout; open
- S2 BattleBannerView _window is default (zero rects, negative inner line widths) until the first Measure; an empty banner text shown visible would skip Measure: call Measure(string.Empty) in the constructor; open
- S3 BattleBannerView summary "centered 190 px from the top" reads as vertical centering; say "its top 190 px down, centered across"; open
