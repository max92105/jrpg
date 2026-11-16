---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB4 (Editor/Battle/TacticListViewModel, SectionRowProblem.FirstInField,
TacticListViewModelTests, TacticListRulesChangedTests, map.md Battle row, EB4/EB5 notes). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): Label prints an emptied threshold as "Below  % HP"; TacticRuleRow.ShownNumber shows
  "?" for the same case, so "Below ? % HP" would match.
- Suggestion (open, optional): no test removes a phase's last rule (allowed by design: Data's
  "phases[i].tactics must not be empty." reaches the page through EnemyCatalogViewModel.Problem -> OfEnemies, EB6).
- Follow-up: the Renumber loop is now in DropTableViewModel, CinematicViewModel (1-based Number) and
  TacticListViewModel; a ListOrder.Renumber(rows, set) could hold it.
- Checked, no finding: list Problem not raised on a row edit is recorded in the EB5 note; Index/Sentence notices
  filtered; Move keeps one watch (tested); RulesChanged raised before Label/Problem notices but Problem is computed;
  FirstOf output unchanged; ToPhase throws for the start list and on a problem.
- EB3b's suggestions (Sentence "?" for an empty box): fixed in EB3b (ShownNumber).
