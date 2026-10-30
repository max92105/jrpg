---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice I2b (the inn asks with its price: RestPlayback question, EventLook).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3136 tests).

- I2a Critical (map.md StillRun parenthetical) checked 2026-10-30: fixed
- Warning, open: EventPresenterInnTests short-of-gold test can't tell the refusal line from "The party rests." (deleting `LineId = InnShortOfGold` stays green); test RestPlayback.LineId directly
- Suggestion, open: EventPresenter.Draw summary omits the inn's question (between message and fade)
- Suggestion, open: EventPresenter class summary "an inn's asks first" -> "an inn's rest asks first"
- Follow-up (carried): Trade/ShopScene 296 lines
