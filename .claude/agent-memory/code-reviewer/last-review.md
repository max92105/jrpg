---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, W1 (Full screen and Alt+Enter)
Round: 1 (verdict: Ready to commit; build 0 warnings, 905/905 tests, format exit 0)

- open (Suggestion): Alt+Enter swallowing the Confirm lives only in JrpgGame.Update, untested; add a W1 hand-check ("Alt+Enter on the dialogue box doesn't advance it")
- open (Suggestion): back-buffer size choice duplicated in ctor and ApplyWindowMode; one private `BackBufferSizeFor(bool fullScreen)`; `DisplaySize()` is a noun, e.g. `ReadDisplaySize()`
- open (Suggestion): record in W1 slice note the test-name prefix deviation (IsPressed_... vs plan's FullScreenShortcut_...) and the extra RightAlt test
- follow-up: full screen sizes to the primary display (DefaultAdapter.CurrentDisplayMode); check by hand on a second monitor of another resolution
