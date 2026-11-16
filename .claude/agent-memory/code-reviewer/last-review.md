---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WD8 (Character sprites window -> Sprites/FieldSpritePage in CatalogDialog via
OpenInDialog from the NPCs entry; MainWindow.FieldSprites.cs deleted; RenderTick.RunWhileLoaded extracted for the
Enemies, Effect strips and Character sprites pages; FieldSpriteCatalogViewModel implements IEditedCatalog).
2026-11-16. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: FieldSpritePage.xaml:67 root DockPanel now wraps a single Grid (OK/Cancel and problem line gone).
- Follow-up, open: page-recipe.md step 3 / line 22 still describes the Loaded/Unloaded pair by hand; name
  RenderTick.RunWhileLoaded so stage 3 pages don't write a fourth copy.
- WD7's Cinematics IsPlaying follow-up: dropped, recorded in slices/WS5.md (note from WD7).
- Checked: the deviation (only the tick released on Unloaded) holds: old OnClosed released the tick, its own CellZoom
  via _zooming.Zoom = null, and handlers on _sprites/_setup, all created per session by Start() or by the page; no
  FieldSpriteWindow / MainWindow.FieldSprites / "Character sprites window" / KeepOnDesktop left in src, tests, map,
  scripts.
