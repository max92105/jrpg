---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-affix-stats, slice ST1b (Affixes section on the Status page: StatusSheet.Affixes, StatusLabels.Affixes,
StatusParametersView.DrawInTwoColumns, status_affixes/status_penetration).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2804 tests).

- Suggestion: StatusParametersView.ResistanceColumnGap now spaces the Affixes' columns too; rename ColumnGap - fixed
- Suggestion: Draw inlines the Resistances'/Affixes' titles while DrawCombat draws its own; let DrawInTwoColumns take
  the SectionTitle (or a DrawSection) so Draw stays at one level - moved to backlog (retro)
- Follow-up: StatusSheetView summary (lines 12-13) "Combat beside Resistances" omits the Affixes section - fixed
