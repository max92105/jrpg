---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice I1 (RestStep.Price; Exploration/InnStay; EventRun.CurrentInnStay/ApplyRest; validator price range; AbsentWhenNullFilter price).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3127 tests).

- Warning, open: "rest every member" loop now in three places (FieldItemUse.RestParty, EventRun.ApplyRest, InnStay.TryPay); add PartyState.RestEveryone() and call it from all three
- Warning, open: validator tests only the refused prices (0, MaxGold+1); an always-refusing FindRestPriceProblem passes; add an accepted-bounds test (Price 1 and Purse.MaxGold return null)
- Suggestion, open: InnStay.TryPay checks the purse twice (CanPay then TrySpend); `_isPaid || !TrySpend(Price)`
- Suggestion, open: no test that a free RestStep writes no "price" key (filter line deletable with suite green)
- Suggestion, open: ApplyRest always returns true; make it void and return true at the case
- Follow-up (carried, grep 2026-10-30): ShopScene 296 lines; ShopListView.Draw 5 params, ShopDetailView.DrawPicker/ShopListView private Draw helpers 5 params
- T3 suggestions (sold-piece notice name, ShopCounter split open state) not re-raised: outside I1
