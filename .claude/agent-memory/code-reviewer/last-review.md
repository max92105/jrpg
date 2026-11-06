---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MS2 (usage checks read every open map: CatalogUsage takes openLayouts,
OpenMaps.Layouts, providers in CombinedHistory/ObjectPlacing/TileBrowser/SheetRemoval), round 1 (2026-11-05).
Verdict: ready to commit.

- Open (Warning): the re-cut warning and Remove sheet's in-use refusal read the hidden open maps only through
  MainViewModel's `() => _openMaps.Layouts` wiring, which no test reaches; suggested a MainViewModelPurgeTests-style test
  (paint a cut tile on map A, open B, Remove sheet / re-cut: names map A).
- Open (Suggestion): TileBrowserViewModel.cs:239 ApplyCut summary "the open map may have changed" -> "an open map".
- Follow-up: CatalogDraft.cs:36-37 Rewriting summary "the open map ... is replaced" (now every open map, ShowRewritten).
- MS1 items all fixed: Rewriting routed through OpenMaps.ReplaceFrom/ShowRewritten; MapEntry.Of param filled;
  NewMapWindow and MainWindow.MapSwitching comments updated.
