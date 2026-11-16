---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice ST (a status acts at the start or the end of its holder's turn).
2026-11-15. Round 1. Verdict: Ready to commit. No Critical.

- Warning, open: StatusCatalogValidator.FindFamilyProblem doesn't compare ActsAt; a family mixing timings loads, and
  StatusSet.Stronger takes the granted record's timing, so a refresh moves the tick (a self-refresh on the holder's own
  turn can tick it twice).
- Follow-up, open: Game/Battle/PlaybackBeatKind.cs:18 Tick summary still says "at the end of its holder's turn".
- Dropped (now in backlog.md:283): EffectValidator groups grants by Status.Id, not FamilyKey.
- Checked: felled-by-start-tick holder skipped (IsAlive guard; Advance skips the fallen; readied turn anchored on it
  still fires via !After.IsAlive); stunned holder ticks then skips then upkeep; IsExtraTurn checked after Advance;
  outcome checked right after the start tick; KeepTurn has no start tick; fresh marks already cleared by the previous
  upkeep; Ready test distinguishes extra from cycle turn; no existing assertion removed.
