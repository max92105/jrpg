---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/LR2.md, slice LR2 (saves keep lasting resources, format 3)
Round: 1 (verdict: Ready to commit)

- Suggestion: SaveGameRepair.cs:101 drop message says "which the class no longer has" also when the class still has the resource but it isn't lasting; say "no longer has as lasting"; open
- Suggestion: no test of a negative saved lasting amount clamped to 0 (lower bound of RepairLastingAmounts); open
- Suggestion: catalog copy giving Sarxian's class a lasting Focus is written twice (FileSaveStoreTests, SaveGameRepairTests) and Clarity twice in GameSessionSaveTests; extract a private CatalogWithLasting() per file; open
- Follow-up: FileSaveStoreTests, SaveGameRepairTests and GameSessionSaveTests are 250-272 lines, near the ~300 limit
