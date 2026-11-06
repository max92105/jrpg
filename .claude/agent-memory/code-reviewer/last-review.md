---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice WP1 (warp destination picker rules: WarpMaps, WarpDestinationViewModel,
WarpArrivalWarning, CollisionShapes.OutlineOf), round 1 (2026-11-05). Verdict: ready to commit.

- Open (Warning): the Touch cell event case warns "Arriving on (0, 2) starts event trap", but Core's EnterMap sets
  _lastCell to the arrival cell, so a cell event never starts on arrival (only on stepping back on). Design question
  for the developer (decision 507): drop the cell-event case or reword.
- Open (Warning): MapId's NotifyPropertyChangedFor(Layout, Warning, Problem) has no test (tests.md relayed-arm rule);
  hall (1, 0) -> closet keeps the cell and would show the stale "can't stand" warning.
- Open (Suggestion): WarpArrivalWarning.Covers repeats PlacedObjectHits.Covers' NPC bounds test; CenterOf computed twice.
- Open (Suggestion): CollisionShapesTests.OutlineOf test sits after the private Placed helper with no blank line.
- Open (Suggestion): fixture tiles picked by position from the test world (First(IsWalkable)); pick by id.
- Fixed (from MS5): GoBack CanExecute true asserted; filter "e" used. Fixed (from MS4): SaveAll_FirstMapRefused added.
