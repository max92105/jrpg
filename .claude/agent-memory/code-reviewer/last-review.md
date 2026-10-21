---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B6b.md, B6b (items in battle: TargetChoiceFor(item), CanUse(item) needs a candidate, BattleItemChoice, IBattleListChoice, CommandMenu Items level, panel DrawList shared)
Round: 1 (verdict: Ready to commit; verify all pass, 1267 tests)

- open (Warning): no test for Confirm on a greyed item (revive, nobody fallen) staying on the list; without the CanUse guard BattleTargetCursor throws on zero candidates (crash).
- open (Warning): ChooseSkill/ChooseItem copy the Up/Down/Cancel block (third copy with ChooseFromList); add MoveUp/MoveDown to IBattleListChoice and share one step.
- open (Suggestion): CommandPanelView.ShowList type-switches on IBattleListChoice implementers with a default throw for an impossible type; branch on _menu.Skills/_menu.Items directly.
- open (Suggestion): ShowList name reads as drawing; it formats the rows' text (e.g. FormatRowsOf).
- open (Suggestion): CommandMenu summary line 12 runs ~170 chars; rewrap.
