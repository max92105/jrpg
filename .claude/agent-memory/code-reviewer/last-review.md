---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WD7 (Cinematics/CinematicCatalogPage in CatalogDialog via OpenInDialog; Closed's
StopTheClock moved to Unloaded; Play still starts the RenderTick; CinematicCatalogViewModel implements IEditedCatalog,
CanApply removed; HasCinematicEditing removed). 2026-11-16. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Follow-up, open: in stage 3 (a page hidden and shown again), Unloaded stops the clock but leaves Preview.IsPlaying
  true, so the page comes back frozen mid-play until Stop; Unloaded could also stop the preview then.
- Declined/not applicable (from WD6): the Loaded/Unloaded RenderTick helper; WD7 starts the tick on Play, so no third
  copy was made.
- Checked: Unloaded after close only lets a frame or two Advance a discarded preview (silent, no media player:
  CinematicPreview plays no audio); OnPreviewChanged starts the tick only while IsVisible, so Apply after close can't
  restart it; no CinematicCatalogWindow / HasCinematicEditing / "Cinematics window" left in src, tests, map, scripts.
