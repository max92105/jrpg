---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E1.md, E1 (error lines collapse, layer list chooses/adds layers, map list by name)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1317 tests)

- open (Critical): MainViewModel.OnOpenMapChanged (refresh LayerEntries on Map change, i.e. after painting) has no test; add paint-at-empty-Z test asserting "empty" mark goes.
- open (Warning): RefreshLayerEntries reassigns LayerEntries on every Map change (each painted cell) and every Z change (incl. clicks in the list) -> ItemsSource reset mid-selection; reassign only when !SequenceEqual.
- open (Suggestion): RefreshLayerEntriesOn(changed, watched) string-pair helper; inline the two property-name checks.
- open (Follow-up): MainWindow.xaml.cs is 475 lines (pre-existing).
