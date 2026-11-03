---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice AE3 (animated Objects preview and composer canvas). Round 1.
Verdict: Ready to commit.

- open (Warning): MapDrawing.IsAnimated untested (gates the Objects preview redraw); add true/false cases to MapDrawingTests
- open (Suggestion): ObjectBrowserView.AnimationTime setter subscribes without unsubscribing the old value
- open (Suggestion): composer canvas calls CatalogDraft.TileAt (linear scan) per painted cell per FramesChanged
- open (Suggestion): docs/map.md ObjectCanvasDrawing line still says CellImageOf; tests' magic `Column <= 28`
- fixed (AE2): Animate panel moved to Tiles/TileAnimationPanel (MainWindow.xaml 296); RemoveAnimationCommand
  re-raised in OnCatalogChanged; TileAnimationTime caches distinct animations per Current change
