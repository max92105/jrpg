# SL. Slowed: a status changes a stat by points

Core + Data + content, about 230 lines (about 150 of them tests), plus `slowed_1`, the Cone of Cold skill and one class
line. Needs SW and T4. Design: README, decisions 797, 807 and 808; design.md, section 2.3.

## Goal
A `statChange` part names a `stat` (accuracy, dodge, crit chance or crowd control resistance) and adds its amount, in
percentage points, to its holder's current stats while held; the result stays at 0 or above, dodge within its cap.
`slowed_1` lowers accuracy by 15 and dodge by 10 (speed orders no turn yet, decision 807); the mage's placeholder Cone
of Cold (ice at Low) leaves it for two turns.

## Changes
- `Core/Statuses/StatusPartKind`: `StatChange` ("points added to one of its holder's stats, its `Stat`").
- `Core/Statuses/StatusPart`: `StatKind? Stat = null` (the last positional, after `DamageType`).
- `Core/Statuses/StatusModifiers`: `Apply` adds each `statChange` through a private `WithStatChange(CombatStats stats,
  StatusPart part)` switch over the four stats (its default arm throws), so `Apply` stays under CA1502; accuracy, crit
  chance and crowd control resistance at 0 or above, dodge within 0 and `StatFormula.DodgeCapPercent`. A static
  `StatusModifiers.ChangeableStats` (the four) is what the validator reads, so the two lists can't drift.
- `Data/Statuses/StatusCatalogValidator`: `stat` required on `statChange` and refused elsewhere; one of
  `ChangeableStats` (`"parts[1]: statChange can't change speed (it changes accuracy, dodge, critChance,
  crowdControlResistance)."`); amount not 0; `statChange` may repeat with another stat, never the same one.
- `Data/Content/AbsentWhenNullFilter`: `(typeof(StatusPart), "stat")`.
- `content/statuses.json`: `slowed_1` (Slowed, harmful, `endOfTurn`, `statChange` accuracy -15, `statChange` dodge
  -10).
- `content/skills.json`: `cone_of_cold` ("A blast of frost at one enemy that slows it for two of its turns.
  Placeholder: the developer writes the real spell.", `Attack.png`, `PlaceholderSkillHex.wav`, mana -4, ice damage at
  `low`, power 0, single enemy, `slowed_1` for 2 turns); `content/classes.json`: the mage's `skillIds` gain
  `cone_of_cold` last.

## Tests
- `Core.Tests/Statuses/StatusModifiersTests` (about 76 lines after WK):
  - `Apply_AccuracyChange_AddsItsPoints`, `Apply_DodgeChange_AddsItsPoints`, `Apply_CritChanceChange_AddsItsPoints`,
    `Apply_CrowdControlResistanceChange_AddsItsPoints`.
  - `Apply_DodgeBelowZero_HeldAtZero`, `Apply_DodgeAboveTheCap_HeldAtTheCap` (both bounds).
  - `Apply_TwoStatChangesOfOneStat_AddUp` (two statuses).
- `Core.Tests/Battle/BattleSlowedTests` (new): `Execute_SlowedAttacker_MissesWhereItWouldHaveHit`
  (`FixedSequenceRandom` drawing between the old and the new hit chance: decision 807's edge, so the test can fail).
- `Data.Tests/Statuses/StatusCatalogValidatorTests`: rows (`statChange` without a stat; a stat on `damageEachTurn`;
  speed; the same stat twice; amount 0) and `Validate_TwoStatChangesOfDifferentStats_Accepted`.
- `Data.Tests/Content/ContentCatalogLoaderStatusTests`: `Load_RepositoryConeOfCold_GrantsSlowed` (ids only).

## Verify by hand
- Run the game, Title > New game, choose Omer, fight the goblins: Cone of Cold (4 mana) hits one goblin with ice and
  leaves an "S" mark named Slowed for two of its turns; its attacks miss Omer more often meanwhile.
