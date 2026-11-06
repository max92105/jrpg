---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice WP4 (Go to a warp's destination: ViewportTransform.CenterOn, Go to button
and GoToWarpDestinationEvent, FollowedWarp on both dialogs, MainWindow.GoToFollowedWarp + DestinationMark), round 1
(2026-11-05). Verdict: fix the Critical.

- Open (Critical): MainWindow.MapSwitching.cs GoToFollowedWarp's one-shot `MapViewport.PreviewMouseDown +=` runs after
  the XAML OnViewportMouseDown on the same Border, which sets e.Handled for every event/paint/pan press, so the gold
  mark stays on most clicks. Fix: call HideDestinationMark() at the top of OnViewportMouseDown and drop the
  subscription (handledEventsToo:true would hide a chained Go to's new mark: the route is snapshot before the dialog).
- Verified OK: OpenCurrentMap's RefitMap runs synchronously inside TryOpenMap before the centering; _isFitPending=false
  stops a later SizeChanged fit. Go to keeps edits exactly as OK (same Problem check, then Apply).
- WP3 carried items: all fixed (shared zoom helper, Pick doc reworded, params filled).
