---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice FX4 (Editor/Statuses/StatusPartRow, StatusViewModel;
SectionRowProblem moved Skills/ -> Catalogs/; notes in FX5, FX6). 2026-11-16. Round 1. Verdict: Ready to commit.

- Warning (open): StatusPartRow.DamageTypes offers no "none" entry, so a typed tick can't go back to typeless (decision
  803, poison_1); asked to rule: fix in the row (a choice record with a none entry first, TierChoice-style), not FX6.
- FX2b carried Suggestion (open, developer's call): the Effects page prints enum names (RestoreHp) not the file's word.
