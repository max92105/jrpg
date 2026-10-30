---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice I3 (editor rest step price; innkeeper charges 20). Last slice of the plan.
Round 1. Verdict: Fix the Criticals, then commit. verify.ps1 all pass (3146 tests).

- I2b items checked 2026-10-30: short-of-gold LineId test (RestPlaybackTests), Draw summary, class summary: all fixed
- Critical, open: EventStepViewModel.cs `// Digits only.` now sits above IsFreeRest (IsFreeRest/Price() inserted between it and WholeNumber)
- Suggestion, open: Price() null = "invalid" while RestStep.Price null = "free"; rename ValidPrice()
- Suggestion, open: RepositoryInnTests InRange(1, int.MaxValue) repeats the validator; NotNull suffices
- Follow-up: RestPlaybackTests.AskingAtTheInn comment says "innkeeper's stay" but uses the BelowInnChest chest
- Follow-up (carried): Trade/ShopScene 296 lines
