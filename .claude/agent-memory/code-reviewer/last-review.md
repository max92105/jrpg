---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MB1 (MapBanner presenter, decisions 528, 529), 2026-11-10. Round 1.
Verdict: fix the Critical, then commit.

- Open (Critical): MapBannerTests.Enter_SameMapAgain_DoesNotRestart asserts Opacity < before; a restart gives 0, also
  < before, so the test passes with the id check deleted. Assert InRange(Opacity, >0, <before).
- Open (Warning): no test of a same-map Enter after the banner ended (Shown null); a check against Shown?.Id instead of
  _lastEnteredId would re-show it on every in-town warp and no test fails.
- Open (Suggestion): MapBanner.WaitTime summary says the warp's fade from black finishes during the wait; the class
  summary (and design) say the clock doesn't move while the warp's event runs, so the fade is already over.
- AL2 items fixed: filters share OptionalFieldFilter.SkipWhen, TryApplySettings summary names the banner, docs/map.md
  line 47 names AbsentWhenTrueFilter.
- start.json residue: the developer's hand test, excluded by the caller.
