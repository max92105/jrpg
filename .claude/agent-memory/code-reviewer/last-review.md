---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice OM3 (New object from map selection), round 1 (2026-11-06).
Verdict: ready to commit (no Criticals).

- Warning (open): ObjectComposerWindow.xaml:68 MapLayerWarning TextBlock not collapsed when null: blank line in every
  New/Edit composer (slice asked "collapsed when null"; MainWindowStyles' ErrorLine comment confirms the gap).
- Warning (open): ObjectComposing class summary still "New object and Edit object ... Tiles panel or Objects panel";
  StartNewFromMap's map-selection job missing.
- Suggestion (open): ObjectComposingLevelTests new [Fact] placed after the private PaintTileOf helper.
- Suggestion (open): no DrawingWarningOf test with both warnings (the " " join).
- Suggestion (open): StartObjectFromMap's `CurrentMap is not { } shown` guard; a selection implies an open map.
- Follow-up dropped: CatalogDraft.TileAt lowest-id-wins on cells cut twice (content has no duplicates today).
- OM2 carried: SheetCellOf shared via SheetCell.Of (fixed); inline copies are in backlog.md:209 (dropped);
  map.md Viewport stray `|` fixed.
