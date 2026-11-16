---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK5 (SkillCosts, ResourceChangeRow, ResourceNeedRow, ResourceChoice,
Costs section on SkillCatalogPage; code uncommitted). 2026-11-16. Round 1. Verdict: Ready to commit.

- Confirmed (implementer's question): opening and OK with no edit records no undo step; CatalogDraft.Skills.cs:27-30
  compares cost lists by content, pinned by CatalogDraftSkillTests.SetSkills_EqualByContentWithFreshLists_RecordsNothing.
- Probed (.NET 10 scratch WPF): cost-row combos write nothing back on skill switch, row removal or close.
- Warning (open): ResourceChoice.AllIn reads content.Classes.Values, a FrozenDictionary; six 4-letter class ids
  (monk,bard,sage,mage,seer,hero) enumerate as monk,hero,bard,mage,seer,sage, so "class order" and Add's first resource
  break; AllIn_ClassesOrder_Kept passes only because ContentOf builds a plain Dictionary.
- Suggestion (open): SkillEditingViewModelTests Apply_Untouched_RecordsNoStep to pin the no-undo path end to end.
- Follow-up: "insert after the chosen row" expression now in 7 places (Cinematics, Events, Loot, Quests, Trade, 2x
  SkillCosts); ListOrder.InsertAfter.
- Follow-up: SkillUsers.LearnedBy lists classes in Classes.Values (FrozenDictionary) order too.
- SK4 Warning (stale file box): fixed (SkillFileChoices.Of appends the skills' names). SK3 Suggestion: fixed. SK6 step 3
  and Cinematics Music box: recorded in SK6.md and backlog, dropped.
