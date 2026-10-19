---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, T2 (Title look)
Round: 1 (verdict: Ready to commit; build 0 warnings, 925/925 tests, format exit 0)

- open (Warning): TitleScene EntryPitch = 98 but its comment's numbers (48 line + 10 + 10 padding + 20 margin) sum to 88; legacy template ignores Padding, so state the real derivation or fix the value
- open (Suggestion): _shadowOffset (3,3) under a comment saying "4 px"; say it is 4 px at legacy Direction 320 (~3.1, 2.6) rounded
- open (Suggestion): plan T2 still unticked with no "Done, with deviations" note (Blurry effect vs decision 10, UiArtFiles only StartMenu.png, Backgrounds folder, no bold, no marker, test name)
- follow-up: none new
