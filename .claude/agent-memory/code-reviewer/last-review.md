---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice WP3 (WarpDestinationWindow, Pick... button and PickWarpDestination routed
event, both event dialogs take MapDrawing), round 1 (2026-11-05). Verdict: ready to commit.

- Open (Warning): WarpDestinationWindow OnZoomChanged/OnPictureMouseWheel are the third copy of the zoom block
  (TileBrowserView, ObjectComposerWindow); backlog.md line ~199 says the third view extracts it into Viewport/.
- Open (Suggestion): PickWarpDestinationEvent doc says the source is the button; at the dialog e.Source is the
  re-targeted EventStepEditor (probe-verified), works only via its DataContext=SelectedStep; reword or use OriginalSource.
- Open (Suggestion): empty `<param name="cellEvent"></param>` / `<param name="events"></param>` in the two dialogs.
- Verified: attached routed event without Add/Remove statics is wired at runtime from BAML (not in Connect), private
  handler reached (scratch WPF probe).
- WP2/WP1 carried Suggestions: committed as is, treated as declined, dropped.
