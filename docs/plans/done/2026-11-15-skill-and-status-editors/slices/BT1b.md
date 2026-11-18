# BT1b. The table's class skill rows

Editor (+ one Core reading), about 300 lines (about 170 of them tests). Decisions 992, 993, 786. Design: design.md,
sections 1 and 2.4. Split from BT1 at its start, then again at its own start (estimate past 320 lines): the boxes
and their view model are [BT1c](BT1c.md). Builds on BT1's `Battle/ClassBattleStats` and `Battle/EffectAmounts` and
row 12's class skill entries (`ClassDefinition.Skills`, each a `ClassSkill(SkillId) { Level }`, absent meaning 1,
ADR 0062).

## Goal
The balance table's class skill rows, headless: every class's damaging and healing skills with what one hit and all
its hits come to at given levels, a given damage modifier added to the modifiers' sum, worked out by the battle's own
rule (a member of the class at that level wearing nothing); a skill the class learns above a level reads "-" at that
level.

## New types
- `Editor/Balance/SkillBalanceRow` (internal sealed record): one skill of one class with its amounts at the table's
  levels: `ClassName`, `SkillName`, `TierLine` ("fire_1 x2", "flat 15"), `Hits`, `IsHeal`, `AmountTexts` (one per
  level: "24", "28 x 4 = 112" with hits, or "-").
- `Editor/Balance/SkillLines` (internal static): a skill's texts whoever uses it, so BT2's enemy rows share them:
  `TierLine(effect)`; `AmountText(effect, user, modifierPermille)`, `EffectAmounts.PerHit` over the user `with {
  DamageModifierPermille = user's + modifierPermille }` (added to the sum, not put in its place: row 9's gear bonus
  starts it), so Core's rule decides what the modifier raises (damage, not heals, decision 786); the total in a long.
- `Editor/Balance/ClassSkillBalanceRows` (internal static): `Of(ContentCatalog content, IReadOnlyList<int> levels, int
  modifierPermille)`: each class in id order (the catalog keeps no file order, as `Skills/SkillUsers` says), its
  skills in `ClassDefinition.Skills`' order, only those `EffectAmounts.ChangesHp` keeps, "-" at a level below its
  `ClassSkill.Level`; each member `ClassBattleStats.At(class, level, content.Rules.Stats)`.

## Changes
- `Core/Battle/EffectAmounts`: public `ChangesHp(EffectKind kind)`, the kinds `PerHit` reads (damage, restoreHp,
  revive), which `PerHit` now guards with, so the table's filter is Core's own.
- `docs/map.md`: the Core Battle line's `EffectAmounts` names `ChangesHp`; an Editor `Balance` line.

## Tests
- `Core.Tests/Battle/EffectAmountsTests`: `ChangesHp_KindWithAnAmount_IsTrue` and `ChangesHp_KindChangingNoHp_IsFalse`
  (theories over the six kinds).
- `Editor.Tests/Balance/BalanceFixtures` (new): a mage and a cleric with six skills of every row shape, built in the
  test over the test world, the mage first in the dictionary; rules with 1 % per Power point on a flat power.
- `Editor.Tests/Balance/ClassSkillBalanceRowsTests` (new): `Of_TieredSkill_AmountsAtTheThreeLevels`,
  `Of_FourHits_ShowsPerHitAndTotal`, `Of_FlatSkill_NamesItsPowerRaisedByDamagePercent`,
  `Of_StatusOnlySkill_NotListed`, `Of_TwoClasses_InIdOrder`, `Of_SkillLearnedAtFive_DashBelowFiveOnly` (levels 4, 5
  and 6), `Of_HealAndDamage_OnlyTheHealMarked`, `Of_MinusTwentyFivePercent_LowersDamageNotHeals`.
- `Editor.Tests/Balance/SkillLinesTests` (new): `AmountText_UserWithAModifier_TableModifierAddedToTheSum`.

## Verify by hand
None: the page comes in BT3.
- **Done, with deviations:** split at its start (estimate past 320 lines): the boxes and their view model are BT1c.
  Core's `EffectAmounts.ChangesHp` added so the table's filter is Core's (27 lines, +7). Editor:
  `ClassSkillBalanceRows` (55; named at review apart from the Classes page's `Party/ClassSkillRow`), `SkillLines` (39,
  shared with BT2's enemy rows), `SkillBalanceRow` (18). Tests 189 lines: `BalanceFixtures` 72,
  `ClassSkillBalanceRowsTests` 86, `SkillLinesTests` 19, `EffectAmountsTests` +12.
