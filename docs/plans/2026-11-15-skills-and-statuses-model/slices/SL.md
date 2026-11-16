# SL. Slowed: a status changes a stat by points, with its own mark letter

Core + Data + Game + content, about 260 lines (about 170 of them tests), plus `slowed_1`, `ice_1`, the Cone of Cold
skill and one class line. Needs SW and T4. Design: README, decisions 797, 807 and 808; design.md, sections 2.3 and 3.3.

## Goal
A `statChange` part names a `stat` (accuracy, dodge, crit chance or crowd control resistance) and adds its amount, in
percentage points, to its holder's current stats while held; the result stays at 0 or above, dodge within its cap. A
status may give its mark a `markLetter`. `slowed_1` lowers accuracy by 15 and crit chance by 5 points and is marked
"L", so it doesn't look like Stun's red "S". **Slowed doesn't slow:** turn order ignores speed (decision 807, a loud
default). The mage's placeholder Cone of Cold (`ice_1`) leaves it for two turns.

## Changes
- `Core/Statuses/StatusPartKind`: `StatChange` ("points added to one of its holder's stats, its `Stat`").
- `Core/Statuses/StatusPart`: `StatKind? Stat = null` (the last positional, after `DamageType`).
- `Core/Statuses/StatusDefinition`: `string? MarkLetter` ("the letter its mark shows; the name's first when absent,
  decision 810").
- `Core/Statuses/StatusModifiers`: `Apply` adds each `statChange` through a private `WithStatChange(CombatStats stats,
  StatusPart part)` switch over the four stats (its default arm throws), so `Apply` stays under CA1502; accuracy, crit
  chance and crowd control resistance at 0 or above, dodge within 0 and `StatFormula.DodgeCapPercent`. A static
  `StatusModifiers.ChangeableStats` (the four) is what the validator reads, so the two lists can't drift.
- `Data/Statuses/StatusCatalogValidator`: `stat` required on `statChange` and refused elsewhere; one of
  `ChangeableStats` (`"parts[1]: statChange can't change speed (it changes accuracy, dodge, critChance,
  crowdControlResistance)."`); amount not 0; `statChange` may repeat with another stat, never the same one; a
  `markLetter`, when present, is one letter or digit.
- `Data/Content/AbsentWhenNullFilter`: `(typeof(StatusPart), "stat")`, `(typeof(StatusDefinition), "markLetter")`.
- `Game/Battle/StatusMarksView.AddMark`: the letter is `status.MarkLetter ?? status.Name[..1]`, upper-cased as today.
- `content/effects.json`: `ice_1` (`damage`, `perPower` 2, `ice`), in id order.
- `content/statuses.json`: `slowed_1` (Slowed, family `slowed`, harmful, `endOfTurn`, `markLetter` "L", `statChange`
  accuracy -15, `statChange` critChance -5).
- `content/skills.json`: `cone_of_cold` ("A blast of frost at one enemy that slows it for two of its turns.
  Placeholder: the developer writes the real spell.", `Attack.png`, `PlaceholderSkillHex.wav`, mana -4, ice damage
  naming `ice_1`, power 0, single enemy, `slowed_1` for 2 turns); `content/classes.json`: the mage's `skillIds` gain
  `cone_of_cold` last.

## Tests
- `Core.Tests/Statuses/StatusModifiersTests` (about 80 lines after WK):
  - `Apply_AccuracyChange_AddsItsPoints`, `Apply_DodgeChange_AddsItsPoints`, `Apply_CritChanceChange_AddsItsPoints`,
    `Apply_CrowdControlResistanceChange_AddsItsPoints`.
  - `Apply_DodgeBelowZero_HeldAtZero`, `Apply_DodgeAboveTheCap_HeldAtTheCap`, `Apply_CritChanceBelowZero_HeldAtZero`.
  - `Apply_TwoStatChangesOfOneStat_AddUp` (two families).
- `Core.Tests/Battle/BattleSlowedTests` (new): `Execute_SlowedAttacker_MissesWhereItWouldHaveHit`
  (`FixedSequenceRandom` drawing between the old and the new hit chance, so the test can fail).
- `Data.Tests/Statuses/StatusCatalogValidatorTests`: rows (`statChange` without a stat; a stat on `damageEachTurn`;
  speed; the same stat twice; amount 0; a `markLetter` of two characters; a blank one) and
  `Validate_TwoStatChangesOfDifferentStats_Accepted`, `Validate_MarkLetterOfOneLetter_Accepted`.
- `Data.Tests/Content/ContentCatalogLoaderStatusTests`: `Load_RepositoryConeOfCold_GrantsSlowed` (ids only).
- `Game.Tests/Battle/StatusMarksViewTests`: `Refresh_StatusWithMarkLetter_ShowsIt` (a record named "Slowed" with "L":
  "L", not "S").

## Verify by hand
- Run the game, Title > New game, choose Omer, fight the goblins: Cone of Cold (4 mana) hits one goblin with ice and
  leaves an "L" mark named Slowed for two of its turns; its attacks miss Omer more often meanwhile, and its turns come
  as often as before.
- **Note from WK:** the Status page shows three skills per class, so Cone of Cold, a fourth, stays off the mage's
  `skillIds` as Enfeeble stayed off the warlock's, until the page scrolls (backlog).
