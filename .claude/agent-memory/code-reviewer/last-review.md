---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice FX5 (Data CatalogProblems.OfStatuses + private CheckReboundSkills;
CatalogDraft.SetStatuses; Editor/Statuses StatusCatalogViewModel, StatusEditingViewModel, StatusViewModel.FamilyKey).
2026-11-16. Round 1. Verdict: Ready to commit.

- FX4 Warning (fixed): StatusPartRow damage type now has a "none" entry (DamageTypeChoice).
- Suggestion (open): no test pins that typing a family does NOT raise Families (the FX6 combo contract).
- Suggestion (open): OfStatuses' `content` param doc omits the items the rebinding reads.
- FX2b carried Follow-up (open, developer's call): Effects page Kind box lists EffectKind enum values (RestoreHp),
  not the file's word; not in backlog.
