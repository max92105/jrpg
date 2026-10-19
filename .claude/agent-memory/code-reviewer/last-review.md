---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, G1-2 (Menu look)
Round: 1 (verdict: Ready to commit; build 0 warnings, 939/939 tests, format exit 0)

- open (Suggestion): UiTheme.MenuRowHeight doc still says "tabs" though tabs now use NavTabShape.Height (both 56); reword to System rows, and G1-1's shared text-centering follow-up is still unshared (note it as deferred in the plan)
- open (Suggestion): FromLegacy channel order unpinned; one golden test, e.g. UiTheme.BarLine == new Color(0x66,0x66,0x66,0x66)
- open (Suggestion): tick G1-2 in the plan and record the hand-off deviations (no slide animation, plain System rows, no header/footer text)
- follow-up: CanopyMask (Rendering, canopy-worded doc) now also masks UI shapes; rename to PolygonMask / generic doc when next touched
