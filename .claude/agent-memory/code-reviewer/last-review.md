---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice H1 (Game/Trade ShopScene + ShopView, shop_hint, ShopSceneTests x2, map.md Trade line).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3032 tests).

- Suggestion: ShopView.BodyPixels is public with no outside reader; make it private until B1 needs it - open
- Suggestion: ShopView's gold change check has no test (MenuHeader exposes ShownGold for one); add when B2 changes gold with the Shop open - open
- Suggestion: ShopView copies ForgeView's frame (column constants/areas, scrim, header title, footer hint) verbatim; 2nd copy, extract to Ui before B1 adds a 3rd reader - open
- Follow-up: GameFlow's EventScreenClosed case still says "The Forge closed" and calls _forges.Close; for E1 - open (carried from L1, still in tree)
- Accepted (slice note): CreateShop stays in E1; ShowGold fed each update; no captures - not raised
