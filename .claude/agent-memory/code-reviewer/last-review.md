---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice T2 (quantity picker; buying stacks; Purchase split out of ShopScene).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3106 tests).

- Warning, open: ShopScene.Notice lets the row's refusal override the purchase notice; buying the most (Down wraps to it, Confirm) greys the row, so "Not enough gold."/"You can't carry more." shows instead of "Bought: Potion x10."; developer's design call
- Suggestion, open: Purchase keeps _picked and Picker as two fields that must agree (two ?? throw guards; _picked stale after Cancel); pair them
- Suggestion, open: ShopSceneQuantityTests has no scene test for Left (StepDown) in the picker
- Follow-up: ShopListView.Draw still 5 parameters; private Draw helpers (ShopDetailView.DrawRow 6, DrawPreview/DrawPicker 5, ~17 in Game) exceed max 4
- T1 carried: SelectedRow! Suggestion fixed (Purchase.BuyPiece takes the row); refusal-id summaries fixed; map.md "Cancel on them" fixed; ShopScene 300-line split done (Purchase)
