---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Out-of-plan fix, decision 592 (amends 523; recorded in done/2026-11-10-map-fixes-banner-and-folders README):
PlacedObjectHits.AllAt ranks by placed Z then draw key; MainViewModel.RefreshLayerEntries unions object Zs.
2026-11-12. Round 1. Verdict: ready to commit. No Critical, no Warning.

- Suggestion (open): "topmost first" wording left in PlacedObjectHits.cs:39 (NextUnder param) and
  PlacedObjectHitsStackTests.cs:7 (class summary, cites only 523); say "in pick order" / add 592.
- Suggestion (open): MainViewModel.cs:239-241 two null-propagating `?? []` could be one `CurrentMap is null` guard.
- GR1 Warning (backlog EventPresenter 293 -> 294): not re-checked, other work; carry only if the next review is GR-plan.
