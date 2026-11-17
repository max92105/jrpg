---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU17 (DamageBonus affix stat, ModifiedFlat cap, Status page gaps).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Follow-up (open): tests/Jrpg.Core.Tests/Statuses/StatusModifiersTests.cs:67 still says "gear's (row 9)" where the
  source comment now names decision 886.
- Checked, no finding: Done note's layout arithmetic (847 of 848) matches the constants; BattleDamageBonusTests'
  expected 48/36/38/40/30 recomputed; ModifiedFlat clamp rows hit at, past and int.MaxValue; only EffectScaling calls
  the formula; no editor/Data per-kind table besides StatNames needed an entry.
