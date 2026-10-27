---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/P3a.md, slice P3a (StatSheet, MemberSheet.Of, StatFormula permille conversions, StatRules nine fields)
Round: 1 (verdict: Ready to commit)

- W1 src/Jrpg.Core/Party/MemberSheet.cs:44-53 positional StatSheet of 7 adjacent ints; only MaxHp, Speed, Primaries, CritChance are pinned through PartyMember; swap passes all tests; named args + one whole-sheet test; open
- S1 tests/Jrpg.Core.Tests/Stats/StatFormulaTests.cs test prefixes CritChance_/Dodge_/CritDamage_/DamagePercent_ name methods now called ...Permille; open (suggestion)
- S2 tests/Jrpg.Core.Tests/Stats/StatFormulaTests.cs:7-8 comment says design.md's placeholders but BaseCrowdControlResistance is 10 (design 0); open (suggestion)
- S3 tests/Jrpg.Data.Tests/Content/GameRulesValidatorTests.cs:15 nine positional ints; named args; open (suggestion)
- F1 src/Jrpg.Game/NewGame/LeaderProfile.cs:27 computes max HP via StatFormula.MaxHp, a second path beside MemberSheet; follow-up
- (P2's W2 "Primaries has no reader" resolved: Primaries now on Sheet, asserted in PartyMemberStatTests)
