---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B4b.md, B4b (CommandListView, BattleScene feeds CommandMenu, Finish on any outcome)
Round: 1 (verdict: Ready to commit; verify all pass, 1204 tests)

- open (Warning): BattleScene.Finish defeat branch (TitleRequested) has no test; add Update_DefeatOnEnemyTurn_RequestsTitle with a catalog goblin whose attack fells Sarxian, Defend, assert TitleRequested + ActiveBattle null.
- open (Suggestion): Update_AttackUntilOver asserts experienceBefore + 40 (repository goblin's experienceReward); read it from the catalog so balance edits don't break it.
- follow-up: none.
