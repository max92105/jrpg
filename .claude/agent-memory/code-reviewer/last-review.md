---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E2.md, E2 (menu bar and controls help window)
Round: 1 (verdict: Ready to commit; verify all pass, 1350 tests)

- open (Suggestion): ControlsHelpWindow Events line "Right click a cell: remove its event" omits that an object on the cell blocks it (HandleEventButton).
- open (Suggestion): F1 pressed twice opens two modeless help windows; keep one and Activate it.
- open (Suggestion): Edit > Delete selected objects runs in every mode, the Del key only in Objects mode; gesture text "Del" suggests they match.
- open (Follow-up): Space + drag pans only if the map had focus when Space went down (HandleViewportKey checks focus; mouse down reads _isSpaceHeld before focusing).
