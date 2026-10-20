---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/C1.md, C1 (battles in the session: VictoryRewards, GameSession.StartPendingBattle/FinishBattle/ActiveBattle/EnemyTactics)
Round: 1 (verdict: Ready to commit; verify all pass, 1140 tests)

- open (Warning): VictoryRewards.Experience "before the split" only tested with one-member parties (BattleStateTests ThreeGoblins, GameSessionBattleTests AfterVictory); total == share, so a share-based Experience passes. Use two members, assert 120 vs 60 each.
- open (Suggestion): FinishBattle after a defeat (returns null, clears encounter) has no test.
