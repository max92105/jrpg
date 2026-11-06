---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice CP1 (PlacedObjectCopies.Make/TopLeftOf, PlaceObjectsEdit), round 1
(2026-11-05). Verdict: ready to commit. Not carried from SN1: its three Suggestions (new slice).

- Suggestion, open: Make_OntoTheMapItCameFrom_LeavesTheOriginalsUnchanged's last assert (target.Objects equal to the
  same instances of an immutable layout) can't fail; the id-disjointness assert is the real check; name it for that.
- Suggestion, open: PlacedObjectCopies summary "a save's memory of the originals never carries over" holds only while
  the originals are on the target; after a cut of the highest-numbered object, a paste back takes its uid and event ids.
- Follow-up: Make/TopLeftOf on an empty list throws (Min); CP2 must never paste an empty clipboard.
- Follow-up (from SN1, still present): window key guards exclude only TextBox, so B/R/F/G eat a focused ComboBox's
  type-to-select.
- Follow-up (from MO2, still present): a wheel zoom mid Shift-drag runs DrawSelectionMarks, resetting the marks'
  transform until the next mouse move.
