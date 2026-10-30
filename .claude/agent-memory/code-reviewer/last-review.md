---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice T1 (buying a stock piece; refused row's reason; the Bought notice).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3095 tests).

- Suggestion, open: ShopScene.Buy(GearPiece) reads SelectedRow! for the name; take the TradeRow instead
- Suggestion, open: MessageIds.ShopRefusedShortOfGold's summary describes all three refusal ids; StackFull/NotForSale have none (Forge gives one each)
- Suggestion, open: docs/map.md Trade line: inserted Confirm clause leaves "Cancel on them" reading as the refused rows; name the tabs
- Follow-up: ShopListView.Draw still takes 5 parameters (predates T1)
- Follow-up: ShopScene at 282 lines; T2's picker will cross 300, plan the split
- L2b carried: Draw-params Critical fixed (ShopView.Place, ShopDetailView 4 params); preview-rebuild Warning fixed (NotEqual + Up_FromGearToConsumable); unbounded while Suggestion fixed (NotSame guard)
