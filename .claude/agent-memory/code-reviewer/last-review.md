---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice E2 (editor openShop step, StepChoices.ShopIds, Shopkeeper preset).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3047 tests).

- Suggestion: docs/map.md Events row says EventStepViewModel is "one view model per step kind"; it is one class for every kind (Kind picks the fields): reword to "one per step, its fields chosen by kind" - open
- Fixed (carried from E1): docs/map.md Flow row no longer lists ForgeTransitions
- Fixed (carried from E1): duplicate GameFlowShopTests.Update_LeaveTheForge_ContinuesTheEvent no longer in tests
- Not raised (slice note explains): OpenShop ToStep throws InvalidFields() on an empty shop, unlike GiveItem/Recruit which pass the empty id through
