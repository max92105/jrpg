---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/K4.md, slice K4 (lead-in/lead-out black, caption band, skip hint)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: EventPresenter lead-out then warp (every intro in content/map-layouts/intro.json): lead-out ends at Blackness 0, warp fade starts from 0, so the intro map pops in at full brightness then fades out; same after a skip. Start the warp at black when coming from black; add Update_AfterLastStillsLeadOut_ContinuesToTheWarp — open
- Warning: skip during the lead-in (RunLeadIn's IsSkipPressed) and "first update's press can't skip" have no test — open
- Suggestion: the update the lead-out ends returns early, so one frame of bare map shows before the dialogue box's first update — open
