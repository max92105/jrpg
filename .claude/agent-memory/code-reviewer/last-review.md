---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice F1d (ForgeMotion fade, ForgeView menu frame, ForgeListView, ForgeDetailView,
forge_hint). Round 1. Verdict: Fix the Criticals. verify.ps1 all pass (2886 tests).

- Critical: ForgeDetailView.DrawPiece draws the rarity label at area.X + RowInset, the name's own x: they overlap - open
- Suggestion: ForgeView FooterGap/BodyHeight copy MenuLayout's private PageArea arithmetic - open
- Suggestion: docs/map.md ForgeScene still says "Cancel asks to close it, once"; now after the fade - open
- Suggestion: ClosingFadeUpdates is a public const on ForgeSceneTests read by GameFlowForgeTests; move to a shared fixture - open
