---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MO0 (moves only: object drag fields and ShowObjectDragAt, ShowGhost,
FinishObjectDrag, IsPastDragDistance, CancelObjectDrag into Shell/MainWindow.ObjectDragging.cs), round 1
(2026-11-05). Verdict: ready to commit.

- No findings. Moved code byte-identical apart from ShowObjectDragAt's summary rewrapped; Jrpg.Core.Maps using moved
  with PlacedObject/ObjectDefinition.
- WP4 carried items: not re-checked here (different slice, moves-only review).
