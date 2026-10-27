---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-26-stats-and-gear-slots, slice B1 (GoblinTrioSimulation: leader's miss/crit/average hit, StartingGear theory). Round 1. Verdict: Ready to commit.

- Suggestion: WearStartingGear ignores TryEquip's result and the comment names the items; a content change that drops a slot's item gives a partly geared run while the Done note still says all nine are worn. Assert the leader wears something in each slot instead of naming items in the comment — open
- Suggestion: Landed == 0 prints NaN for crit/average hit; harmless today (every class hits) — open
