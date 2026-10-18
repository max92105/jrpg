---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11h-3 (New maps)
Round: 1 (verdict: Ready to commit; build, 770 tests, format exit 0 all pass)

- open (Warning): MainViewModel.TryCreateMap size gate untested; without it ToMap throws InvalidOperationException (editor crash) on Create with bad size; add TryCreateMap_InvalidSize_OpensNothing
- open (Suggestion): MainViewModel.StartNewMap gates on Painting but reads Catalog!; doc says "content didn't load" (also art); gate on Catalog/Painting consistently with doc
- open (Suggestion): sorted map id list built twice (ctor line 48, Save line 204); one private SortedMapIds()
- open (Suggestion): tick 11h-3 in plan; note "New map..." button stays enabled when content is broken (does nothing)
- verified: MapIds rebinding (UpdateTarget then SelectedItem; SelectionChanged reenters TryOpenMap with same id -> true), draft not in list selects nothing, refused switch from draft leaves list unselected, empty Layers pass MapLayoutValidator and ContentWriter (map not in catalog), case-insensitive id collisions, ForgetSavedState keeps draft dirty
