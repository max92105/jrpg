---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/P4b.md, slice P4b (worn gear adds to MemberSheet; PartyMember rebuilds sheet on Wear/TakeOff; GameSession wears before RestoreProgress; SaveGameRepair clamps HP with worn bonuses)
Round: 1 (verdict: Ready to commit)

- W1 src/Jrpg.Core/Party/MemberSheet.cs:26 sheet MaxHp can drop below 1 with validator-legal negative bonuses (two -100 maxHp or Constitution items), then Vitals.ResizeMax throws on Wear/Load and Math.Clamp(hp, 0, negative) throws in SaveGameRepair; floor at 1 or ask developer; open
- W2 src/Jrpg.Core/Party/PartyMember.cs:44-47,77-79,226 summaries stale: Sheet "at their level... Rebuilt on level-up", RestoreProgress doesn't say worn items must go on first, TakeClassStats "class's numbers"; open
- W3 src/Jrpg.Core/Party/MemberSheet.cs:11 PermillePerPercent now a third private copy (StatFormula, Resistances); expose one percent-to-permille in StatFormula; open
- S1 PartyMember.TakeClassStats(ClassStats): every caller passes StatsAt(Class, Level), and it applies gear now; parameterless RebuildStats(); open (suggestion)
- S2 cap-once-after-gear (dodge/resistances) untested with a negative bonus over cap; open (suggestion)
- S3 StatBonuses.Plus keeps kinds summing to 0, so sum != None and Values lists a 0; open (suggestion)
- F1 worn bonuses summed in PartyMember.WornBonuses and SaveGameRepair; G1a-2's nine slots must update both; follow-up
- F2 (carried) src/Jrpg.Game/NewGame/LeaderProfile.cs:27 second max-HP path; MemberSheet is public now; follow-up
- F3 (carried from P4a) design.md:105 stale derived kind names; follow-up
