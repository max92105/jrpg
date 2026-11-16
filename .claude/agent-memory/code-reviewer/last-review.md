---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK4 (SkillCatalogPage, ReadSkillChoices, Skills... opener and menu
entry, smoke-script entry; code uncommitted). 2026-11-16. Round 1. Verdict: Ready to commit.

- Ruled (implementer's question, probed on .NET 10 WPF): a missing Icon/Sound/Strip file is never written back as null
  or changed; the skill keeps its value through switches and close.
- Warning (open): switching from a skill with a listed file to one whose file is missing leaves the box showing the
  previous skill's file (probed); fix by appending the draft's missing names to the choices in ReadSkillChoices.
- Follow-up: Cinematics Music box has the same stale display (its MissingNote names the file).
- Carried from SK3, Suggestion (open, optional): AddSkill_EmptyCatalogOfTiers_StartsAFlatDamage doesn't assert the
  damage type.
- Carried from SK3, Follow-up: SK6 step 3 ("Tier none ... OK lights") contradicts Data's tier rule (decision 704).
