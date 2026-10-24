---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/K2c.md, slice K2c (game shows a still step)
Round: 1 (verdict: Ready to commit)

- Warning: EventPresenter.ShowStill sets _still=null on ContinueEvent, so between two consecutive stills one frame draws the bare map (Draw sees no still); fix by starting the next still's playback in the same update or drawing black while CurrentEventStep is a StillStep (open)
- Suggestion: StillPlayback.Still reads as `still.Still.ImageFile` at the call site; `Step` would read better (open)
- Follow-up: StillPlayback.Opacity gives NaN for a Duration under 1 tick (validator allows Seconds > 0, e.g. 1e-9) (open)
- Follow-up: carried from K2a/K2b: SkipEventPresentation end-of-event test; ExplorationState.cs near 300 lines; Arrive events gold in editor (open)
