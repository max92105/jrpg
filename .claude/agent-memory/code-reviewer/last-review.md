---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK7 (Skills/SkillGrants, StatusGrantRow, StatusChoice,
Events/ListOrder.InsertAfter, Statuses band on SkillCatalogPage; code uncommitted). 2026-11-16. Round 1.
Verdict: Ready to commit.

- Warning (open): tests' SkillFixtures.NoChoices now offers poison_1 and its record (Bolt's grant must resolve);
  the name says no choices while grants' Add is enabled. Proposed rename PoisonOnlyChoices (38 uses, sed).
- Warning (open): SkillGrants.Problem re-implements SkillCosts.FirstProblem ("<list> row N: ..."); share one helper.
- Suggestion (open): SkillViewModel.TypedRecord comment names costs' rows and effect's boxes as left out, not the
  grant rows.
- Confirmed: untouched skill with two grants records no step (test resolves over value-equal copies); same XAML
  combo pattern as the cost rows (SK5 probed).
- SK6 items: IsTiered gone and _firstDamageTier reads Choices.Tiers (fixed); greyed Damage type display
  suggestion not carried.
