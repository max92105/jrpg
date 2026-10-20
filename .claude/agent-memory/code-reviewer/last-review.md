---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B1a.md, B1a (FadeGate: request returned mid-fade waits; second one throws)
Round: 1 (verdict: Ready to commit; verify all pass, 1145 tests)

- open (Suggestion): a held request outlives the scene that returned it; if the due-at-black request replaces the stack, the held one (e.g. MenuClosed) is applied to the new stack. Unreachable today (no input during fade); consider dropping `_waiting` when the due request is applied, or document.
- open (Suggestion): GameFlow's order (ApplyRequestOnceBlack before TakeReleased) has no GameFlow-level test; accepted by the slice deviation, revisit when the battle scene can return a request mid-fade.
