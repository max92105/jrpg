---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/T3.md, slice T3 (title entrance and a clear selection; decision 252)
Round: 1 (verdict: Ready to commit)

- S1 TitleScene.DrawEntries also draws the footer; split a DrawFooter(batch, opacity) out so the name stays true; open
- S2 Footer position recomputed every Draw; compute it once in the constructor beside _labelPositions (slice: places computed once); open
- S3 TitleScene.cs ends with an extra blank line after the closing brace; open
