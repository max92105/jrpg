---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/P3b.md, slice P3b (DamageType, DamageTypes.IsPhysical, Resistances, StatFormula.ResistancesFrom/StatusPermille/ExtraStatusTurns, four StatRules fields)
Round: 1 (verdict: Ready to commit)

- W1 tests/Jrpg.Core.Tests/Stats/ResistancesTests.cs Clamped_AboveCap/BelowFloor and StatFormulaTests ResistancesFrom_HugeConstitution assert against `CapPercent * 10` / `FloorPercent * 10`, so a wrong cap (100 = immunity) passes; pin 750 / -1000; open
- S1 src/Jrpg.Core/Stats/Resistances.cs:15 second private PermillePerPercent beside StatFormula's; open (suggestion)
- S2 Done note doesn't record Cap/Floor named CapPercent/FloorPercent (slice and design.md:34 say Cap/Floor); open (suggestion)
- F1 design.md:105 still lists DamagePercent, StatusPercent, PhysicalResistance, MagicalResistance; follow-up
- F2 (carried) src/Jrpg.Game/NewGame/LeaderProfile.cs second max-HP path beside MemberSheet; follow-up
