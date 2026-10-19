---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, T1a (Scene requests and the flow's fade)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, 911/911 tests, format exit 0)

- open (Critical): SceneStackTests.cs RecordingScene doc comment mangled (empty name/calls param tags, stray blank line, two `///` on one line); format didn't catch it
- open (Suggestion): SceneFactory.Exploration is a noun method; verb phrase e.g. CreateExploration
- open (Suggestion): full-screen black overlay draw duplicated in GameFlow.Draw and EventPresenter.Draw; one shared draw for a ScreenFade
- follow-up: a scene returning a request while the flow's fade plays makes ScreenFade.Start throw; unreachable today (no input while fading), decide queue/ignore when G1 adds menu requests
