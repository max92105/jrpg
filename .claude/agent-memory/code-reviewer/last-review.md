---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL6b (CL6's split: Skills tab commands, add box, XAML; decisions 949,
1014). 2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Probed (scratch net10 WPF project `cl6bprobe`, real ClassCatalogPage + repo content, mage): no binding trace line;
  focus in row 2's Level box chooses it; Move keeps the ListBox selection; Add clears the box (25 left) and the line
  names the resource; Remove selects the next; each class keeps its own add-box pick across switches.
- Suggestion: SkillsToAdd by-name test passes with an id sort too (fixture names sort like ids) — open
- Suggestion: map.md Party row "`Changed` for the typed fields only" beside the new list-change clause — open
- Follow-up: TypedRow ListBoxItem style now in four pages (Item, Skill, Class keyed; DropTable inline) — open
- Follow-up: ClassCatalogPage.xaml.cs summary names only the Stats tab (no Levels, no Skills) — open
- From CL6: missing-skill row through ToClass + Data message — fixed (Problem_IdTheDraftLacks_ShowsDataMessage).
