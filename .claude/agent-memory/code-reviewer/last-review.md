---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B5a.md, B5a (TurnStage, BattlePacing, BattleTurnSequence stage machine, BattlersView step offset, BattleScene ShowStage)
Round: 1 (verdict: Ready to commit; verify all pass, 1218 tests)

- open (Suggestion): BattleScene._shown hardcodes (1, StepForward); initialize from (_sequence.Turn, _sequence.Stage) after the sequence is built.
- open (Suggestion): no test pins StepBack's share falling (e.g. 0.1 s into StepBack -> 0.5) or Defend's zero hold; Run's zero hold is only implied by Submit_Flee_IsOver.
- open (Suggestion): Update_LongDelta test relies on goblin A's attack producing EffectApplied (1.0 s hold); a comment says so, fine, but a miss/defeat change in Core would shift it.
- follow-up: none.
