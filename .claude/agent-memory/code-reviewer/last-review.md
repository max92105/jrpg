---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice NG3 (Database > New game: the page). 2026-11-17.
Round 1. Verdict: Ready to commit. No Critical, no Warning, no Suggestion.

- Runtime probe (scratchpad ng3probe, real page + theme + repository content): 0 binding/resource trace lines;
  a Quantity box's focus chooses its row; Up/Remove/Add act on the chosen row; the sprite box keeps a missing value
  and its ItemsSource never changes (same DataContext), so the CL9d trap doesn't apply; nothing written back on close.
- NG2c items: Warning (record-or-missing lookup, add-box query) fixed (RecordChoice.Of / RecordChoice.ByName in all
  three); Suggestion two bad quantities fixed (Problem_TwoQuantitiesNotNumbers_NamesTheFirstRowsItem); Suggestion
  kept-list check fixed (ListOrder.KeptIfEqual); Follow-up StartingItemRow/ClassSkillRow dropped (backlog.md lists it).
