---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL5b (CL5's split: the Levels tab; ClassViewModel LevelRows and
LevelsProblem, ClassCatalogPage Levels tab, ClassFixtures.World, ClassViewModelLevelTests).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Probe (scratch net10 WPF, the tab's XAML verbatim): no binding error once selected; header and rows share widths;
  18 of 99 rows realized; LevelRows read once per notice, also while the Stats tab is shown.
- Suggestion: ItemsControl's ScrollViewer.CanContentScroll and VirtualizingPanel.IsVirtualizing attributes are no-ops
  (template ScrollViewer sets its own; IsVirtualizing defaults true) — open
- Follow-up: Game/NewGame/LeaderProfile.cs:27 still inlines ClassSheet.At(class, 1, rules) — open (carried from CL5)
- Follow-up: eight stat labels in page order in ClassStatsBoxes.Problem and ClassLevelPreview.StatsOf — open (from CL5)
- From CL5: ClassLevelPreview.ProblemAt guard order — fixed (range check is first)
- From CL5: StatFormula.MaxHp / AccuracyPermille wrap — dropped, backlog.md lists it
