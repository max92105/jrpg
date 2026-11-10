---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice UO3 (Events mode reaches the ground under objects, decision 526),
2026-11-10. Round 1. Verdict: ready to commit. No Critical, no Warning.

- Open (Suggestion): EventEditingCellTests never asserts the chest covers (4, 9) (e.g. StartEditingAt(4.5, 9.5) gives
  the chest's events), so the "UnderAnObject" scenario is unproven by the tests themselves.
- Open (Suggestion): ControlsHelpWindow.xaml:102 Ctrl gestures appended to the plain-click line, explained before the
  plain right click line 103; own line after 103 reads in order.
- UO2 items all fixed (Forget on layer change at ObjectPicking.cs:29, TopAt deleted, param tags filled,
  IsLockHidingObjectsAt bool + note text in ObjectPicking); the locked-selection Suggestion not carried (new slice).
- start.json residue: the developer's hand test, excluded by the caller.
