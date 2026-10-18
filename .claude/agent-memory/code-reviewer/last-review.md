---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11h-1 (Split the editor's main view model)
Round: 1 (verdict: Ready to commit; build, 736 tests, format exit 0 all pass)

- open (Warning): TilePaintingViewModel.cs:57 EndStroke only called by tests; MainWindow.xaml.cs:338 ends strokes via _viewModel.CurrentMap?.EndStroke() while starting them via Painting; call Painting.EndStroke() or drop the method
- open (Suggestion): MainViewModelTests TryOpenMap_AnotherMap_PaintsAndPlacesOnIt only asserts the hand-off; rename TryOpenMap_AnotherMap_HandsItToPaintingAndPlacing
- open (Suggestion): LayerChoiceViewModel.IsOthersDimmed reads oddly; AreOtherLayersDimmed / IsOtherLayersDimmed
- open (Suggestion): tick 11h-1 in the plan, note LayerChoiceViewModel (unplanned type) and that clicks with no open map are now ignored
- verified: all XAML binding paths updated; `!` accessors (Painting/Placing) unreachable with null (all mouse/key/preview paths guard CurrentMap or Painting first; CurrentMap only non-null when Painting/Placing were built)
