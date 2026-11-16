---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN5 (Animation/CellStrip, Animation/StripFromCellsViewModel,
Sprites/CellPicking extracted from FieldSpriteSetupViewModel.PickCellAt and FacingSetup.CellsText). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): PickRow's cut-is-a-problem guard (e.g. ColumnsText "5" on 192 px) has no test; without it the
  row adds cells of a cut that doesn't split (tests.md: every no-op guard gets a test).
- Suggestion (open): StripFromCellsProblemTests reaches SixByTwo through `using static` of the other test class; a
  small shared fixture type (StripFromCellsFixtures) would decouple the two files.
- Checked, no finding: the extraction matches the old PickCellAt exactly (same < 1 and < 0 guards, Math.Floor, < Columns
  and < Rows); x == sheet width gives column == Columns, refused (tested at 192 and 64); "con.png" is a regular file on
  this Windows 11 machine (GetFullPath and a write probe); ".." becomes "...png", valid; replacing an existing file is
  AN7's (asks before replacing).
- AN4 carried: its suggestions dropped (committed slice); its hit-frame Follow-up dropped (backlog.md lists it).
