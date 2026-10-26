---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/MO1.md, slice MO1 (MenuMotion: menu slide-in and closing fade)
Round: 1 (verdict: Ready to commit; flow checked: GameFlow pops on MenuClosed whenever it comes, SceneStack draws all scenes, FadeGate only holds during a fade, when the menu gets default input)

- W1 MenuMotionTests — BG1's regression test (no time counts before the first draw) went with OpeningFadeTests and has no MenuMotion replacement; add FrameOpacity_AdvanceBeforeFirstDraw_StaysClear; open
- S1 MenuMotionTests — TabOpacity untested (fade with slide, out with closing); open
- S2 GameMenuScene `_isClosedRequested` reads oddly; `_hasRequestedClose`; open
