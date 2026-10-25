---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/LK3.md, slice LK3 (formation slide, aura floor glow, extra-turn mark)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: PartyStatusView `_drawOrder` no longer the draw order (Draw iterates by panel index; field only feeds PlacePanels stacking); rename e.g. `_panelAtPlace`; open
- Warning: PartyStatusView.Refresh summary "allocating nothing when none did" now false: PlacePanels allocates `new int[]` each refresh; reuse a scratch array or fix the summary; open
- Suggestion: FormationSlide.Share public with no outside reader; make private; open
