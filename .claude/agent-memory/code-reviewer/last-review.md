---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice XP2b (Add level, Remove last level, max level note; split from XP2).
2026-11-17. Round 2. Verdict: Ready to commit. No Critical, no Warning.

- Round 1 Warning (note measured against the session's start, cleared by an Apply): fixed. CatalogDraft.SavedMaxLevel
  is set at load and in MarkSaved and passed by ExperienceEditingViewModel.Start; Start_LastLevelRemovedAndApplied
  tells the old code from the new.
- ReplaceAfterRewrite not setting SavedMaxLevel: declined, not reachable. All three callers refuse while unsaved
  (SheetRemoval.AskPurge IsDirty, map rename and folder rename HasUnsavedChanges), and none changes the table.
- XP2 Suggestion (ExperienceFixtures' Classes override): fixed.
- ListItemWatch summary missing users: not raised, backlog.md line 385 holds it.
