---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice ML0 (moves only: the map list into Shell/MapListPanel), 2026-11-10.
Round 1. Verdict: ready to commit.

- Open (Warning): docs/map.md:80 Shell row still describes `MainWindow.MapSwitching` as "(the map list, its filter, and
  a warp's Go to ...)"; the list and filter now live in `MapListPanel`. Reword to "(the open map's row kept selected,
  Find reaching the filter, and a warp's Go to ...)".
- Open (Suggestion): MapListPanel.xaml.cs `_selectedMapId` holds the open map's id (the fallback for a refused switch),
  not the list's selection; `_openMapId` says so.
- Open (Suggestion): MainWindow.MapSwitching.cs:10 `<see cref="MapListPanel"/>` inside MainWindow binds to the field
  of the same name, not the type; `Shell.MapListPanel` or plain text.
- MB3 items all fixed (hairline drawn before the no-level return, TextWidth, rounded slide, HairlineHeight constant,
  design.md tunables no longer name map_banner_level).
- start.json residue: the developer's hand test, excluded by the caller.
