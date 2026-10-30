---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice G4 (gold with its coin: Ui/GoldAmountView, MenuHeader gold, VictoryScreen.Gold,
VictoryScreenView.RewardRows(VictoryScreen) with an IsGold row, text battle_victory_gold, Icons.Gold). Round 1.
Verdict: Ready to commit. verify.ps1 all pass (2939 tests).

- Warning: VictoryScreenView.cs DrawRewards - the second reward bar (y 344-408) meets the first card row's "Level Up!" pop (center y ~399) and the Items heading (y 404) - open
- Suggestion: RewardRows builds the EXP row twice (both ternary branches) - open
- Suggestion: _rewardRows/_rewardLabels/_rewardValues are three parallel collections; one drawn-row record - open
- G3 carried Critical (EnemyCatalogValidator ADR 0027 comment under FindGoldProblem) - fixed (grep: comment at line 44 above FindResistanceProblem)
