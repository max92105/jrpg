---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN1a (Editor Animation/FramePlayback + FramePlaybackTests,
FramePlaybackNoticeTests; map row Animation; AN1b note). 2026-11-16. Round 1. Verdict: Ready to commit.

- Suggestion (open): no test pins "Play after a mid-frame pause carries on from the paused moment" (pause at 150 ms,
  play, Advance 60 -> frame 2); a ShowFromStart added to TogglePlaying would pass every test.
- Suggestion (open): the `_elapsed` comment says the modulo keeps the frame independent of run length; LoopFrameAt's
  own `% frameCount` already does that; the modulo only bounds the TimeSpan. Reword the why.
- Suggestion (open): FrameCount summary "From 0: no frames to show..." reads as a range; "0 means no frames...".
- FX6 Warning (row-chooses-on-focus third copy): fixed, Catalogs/RowFocus attached property.
- FX6 / FX2b Kind box items: dropped, listed in docs/plans/backlog.md (line ~303).
- FX6 Family box comment Suggestion: not carried (committed slice, Suggestion only).
