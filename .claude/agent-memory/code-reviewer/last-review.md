---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MB2 (MapBannerView drawn plain, FieldOverlays, decisions 527-529),
2026-11-10. Round 1. Verdict: fix the Critical, then commit.

- Open (Critical): tests/Jrpg.Game.Tests/Exploration/StartMapScenarios.cs:131-136, the new SceneOf overload's doc
  comment is mangled (empty exploration/catalog params, a blank line inside, "</param> /// <param" on one line).
- Open (Warning): no test fails if ExplorationScene.Update drops `_overlays.BannerView.Update()` (Draw then draws
  nothing all game); ExplorationSceneBannerTests read only MapBanner, not the view the scene drives.
- Open (Suggestion): Update_SameMapAfterTheBannerEnded_DoesNotRestart cannot fail through the scene (no Enter without
  a map change; MB1's Enter_SameMapAfterTheBannerEnded_ShowsNothing covers the id check).
- MB1 items fixed: Enter_SameMapAgain asserts InRange(0.01, before), Enter_SameMapAfterTheBannerEnded test exists,
  WaitTime summary corrected.
- start.json residue: the developer's hand test, excluded by the caller.
