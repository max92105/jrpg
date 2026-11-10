---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice UO2 (Dim and lock other layers, decisions 522, 525), 2026-11-10.
Round 1. Verdict: ready to commit.

- Open (Warning): ObjectPicking.cs:47-52 the "Locked: only layer z 2 takes clicks" note (and "2 of 3 objects here")
  stays after the box is unticked or the layer changed; Forget() on Layer's Z/AreOtherLayersDimmed change + one test.
- Open (Warning): PlacedObjectHits.cs:34 TopAt has no production caller left (EventEditingViewModel moved to AllAt);
  only ObjectEditingTests 162-180 use it. Delete it, port the tests to AllAt.
- Open (Suggestion): EventEditingViewModel.cs:13-14 empty `<param>` tags; fill them or drop the `layer` one.
- Open (Suggestion): LockNoteAt builds note text in ObjectPlacingViewModel while ObjectPicking owns the other note
  text, and scans AllObjectsAt twice; a bool `IsLockHidingObjectsAt` + text in ObjectPicking.
- Open (Suggestion): a locked-layer object selected before ticking stays selected (Delete, arrows, panel still act).
- UO1 items: note-survives-move fixed (Picking.Forget at ObjectPlacingViewModel.cs:251); start.json residue is the
  developer's hand test, excluded by the caller; the two UO1 Suggestions not carried (new slice).
