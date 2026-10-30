---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice I2a (pure refactor: Game/Exploration/RestPlayback out of EventPresenter).
Round 1. Verdict: Fix the Criticals, then commit. verify.ps1 all pass (3130 tests).

- Critical, open: docs/map.md:64 Exploration line: `RestPlayback (...)` inserted between `StillRun` and StillRun's "(a run of stills, opening and closing on black)", which now describes RestPlayback; move RestPlayback after StillRun's parenthetical
- I1 carried items checked by grep 2026-10-30: PartyState.RestEveryone used by all three (fixed); validator accepted-bound test MapEventValidatorTests:130 (fixed); InnStay.TryPay single check (fixed); ApplyRest void (fixed)
- Follow-up (carried): Trade/ShopScene 296 lines
