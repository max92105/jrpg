---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M3a (Interact and swapped objects)
Round: 1 (verdict: Ready to commit; build 0 warnings, 864 tests pass, format exit 0)

- open (Suggestion): record in the M3a slice note that the Core tests use the lever fixture (CurrentDefinitionOf_AfterSwap_ReturnsSwappedDefinition) instead of the plan's door-named test
- open (Suggestion): StartMapScenarios also carries HoldingLeft/OneUpdate, which aren't start-map scenarios; fine for now, split if it grows
- follow-up: M3b must stop the Confirm that closes a message box from re-triggering TryInteract in the same ExplorationScene.Update (presenter runs first, then IsBusy is false and the same press interacts again)
- follow-up: M3a checkbox, hand checks and test-name deviation not yet in the plan (DoD 5)
- verified: _currentDefinitionOf delegate field avoids per-frame instance method-group allocation; CanopyTextures built from all catalog objects so swapped ids resolve; MapDrawList swap test fails with the old dictionary lookup (tree base 6 vs stump base 3 around feet 4.5)
