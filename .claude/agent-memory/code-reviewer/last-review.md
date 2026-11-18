---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice BT3 (the plan's last): Editor `Balance/BalanceTablePage`,
`BalanceTableEditingViewModel`, `BalanceTableViewModel : IEditedCatalog`, registration in Shell, menu entry, smoke list.
2026-11-18. Round 1. Verdict: Ready to commit.

- Carried from BT2b, fixed: `AllRead` is now `ReadAll`.
- Carried from BT2b, fixed: the Power refusal theories are merged into `BoxesProblem_PowerRefused_...`.
- Carried from BT2b, fixed: `MaxTypedPower` comment now says an amount past 1,000,000 shows as 1,000,000.
- Carried from BT2b, dropped: "damage, fire" spelling share was a third-copy-only suggestion; only one copy in Balance.
- Suggestion, open: `BalanceTablePage.xaml` at 289 lines; the next tab or column moves each tab into its own UserControl.
- Suggestion, open: `ChosenKey_IsNull` lacks a scenario (`ChosenKey_NewTable_IsNull`).
- Follow-up: backlog.md line 197 ("the first balancing tool worth building is a read-only ... table") is now built.

Checked: every row-template binding matches its record (SkillBalanceRow, EnemySkillBalanceRow, ClassBalanceRow,
EnemyBalanceRow, TierBalanceRow, IsHeal on all three AmountCell users); indexed lists are collection expressions
(IList, same as the Skills tab the smoke run showed). Workspace has no dirty flag: Apply is a lit no-op (deviation noted).
