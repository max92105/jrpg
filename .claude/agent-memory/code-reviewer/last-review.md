---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice AE2 (Animate panel, Apply/Remove, TileAnimationTime). Round 1.
Verdict: Ready to commit.

- open (Warning): MainWindow.xaml 283 -> 309 lines; move the Animate panel into a Tiles/ UserControl
- open (Warning): RemoveAnimationCommand CanExecute not re-raised on CatalogDraft.Current change (Ctrl+Z/Ctrl+Y while
  open leaves it stale); notify in OnCatalogChanged, add a test
- open (Suggestion): TileAnimationTime.Advance runs Select/OfType/Distinct over every tile each render tick; cache
  the distinct animations on Current change
- open (Suggestion): no test raises CanExecuteChanged for Apply/Remove on Open or a typed box
- fixed (AE1): ProblemOf overflow (subtracted), Close test, panel closes when its sheet leaves the catalog
- dropped (AE1): ArtReferenceCheck.LastFrameColumn follow-up is in backlog.md; CanOpen SheetChoices arm suggestion
