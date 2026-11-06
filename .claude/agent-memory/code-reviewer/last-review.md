---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MS1 (open maps stay open: OpenMaps, MapEntry.Of, TryOpenMap/TryCreateMap never
ask, " *" marks, ConfirmClose naming every map), round 1 (2026-11-05). Verdict: fix the Criticals.

- Open (Critical): MainViewModel.cs:79 Content.Rewriting assigns CurrentMap = new MapViewModel(...) outside _openMaps;
  after a purge the shown map's edits raise no DirtyChanged, HasUnsavedChanges/ConfirmClose ignore it (closes without
  asking, edits lost), and switching away and back shows the stale pre-purge VM. Fix: route through OpenMaps (pull MS2's
  ReplaceFrom/ShowRewritten forward, or a minimal replace) + a MainViewModelPurgeTests test (purge, paint, ConfirmClose asks).
- Open (Suggestion): MapEntry.Of empty `<param name="map"></param>`.
- Follow-up: NewMapWindow.xaml.cs:7 "or the user kept the open map"; MainWindow.MapSwitching.cs:14 "a refused switch" and
  :30 "A save rebuilds the list" now stale (an edit's mark rebuilds it too).
- Checked: list reselect after MapEntries replacement, re-entrant SelectionChanged during Show's EndStroke, new map saved
  listed once, StartNewMap taken ids.
