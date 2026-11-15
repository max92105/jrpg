# WK. Weakened: a status lowers the damage its holder deals

Core + Data + content, about 230 lines (about 140 of them tests), plus `weakened_1`, the Enfeeble skill and one class
line. Needs SW and T4. Design: README, decisions 783, 786, 806 and 808; design.md, section 5.

## Goal
A `damageDealtChange` part adds its amount, a percent, to its holder's damage modifiers' sum, decision 704's
"1 + the sum of modifiers": tiered damage becomes Power x `perPower` x (1 + sum), the basic attack its weapon damage x
Power's percent x (1 + sum); heals and items don't change. `weakened_1` (-25 %) and a placeholder warlock skill,
Enfeeble, show it.

## Changes
- `Core/Statuses/StatusPartKind`: `DamageDealtChange` ("a percent added to the damage its holder deals, decision 786").
- `Core/Stats/CombatStats`: `int DamageModifierPermille { get; init; }` ("the sum of the modifiers on the damage the
  combatant deals, in permille (decision 704's "1 + the sum"); its statuses' damage dealt changes; 0 by default").
- `Core/Statuses/StatusModifiers.Apply`: sets `DamageModifierPermille` from the `damageDealtChange` sum (its `SumOf`
  from SW); the summary names Weakened.
- `Core/Effects/DamageFormula.TieredAmount(int power, int perPower, int modifierPermille)`: `power x perPower x (1000 +
  modifierPermille) / 1000` in a `long`, 0 at least, rounded down once; the comment cites the sheet's example.
- `Core/Battle/EffectScaling.ScaledFor`: a tiered damage passes the user's `DamageModifierPermille`, a tiered heal 0
  (decision 786); a flat damage (the basic attack) goes through a new `DamageFormula.ModifiedFlat(int raw, int
  damagePermille, int modifierPermille)`, `raw x damagePermille x (1000 + modifiers)` in one `long` product rounded
  down once (0 at least); a flat heal keeps `StatFormula.Scaled`.
- `Data/Statuses/StatusCatalogValidator`: the `damageDealtChange` arm: above -100, not 0.
- `content/statuses.json`: `weakened_1` (Weakened, harmful, `endOfTurn`, `damageDealtChange` -25), in the writer's
  layout and id order.
- `content/skills.json`: `enfeeble` ("Saps one enemy's strength for three of its turns. Feeds instability. Placeholder:
  the developer writes the real spell.", `Attack.png`, `PlaceholderSkillDark.wav`, instability +2, `grantStatus` at a
  single enemy, `weakened_1` for 3 turns); `content/classes.json`: the warlock's `skillIds` gain `enfeeble` last.
  Check `Jrpg.Game.Tests` for a test counting Sarxian's skills (the command menu's tests pick by id: safe).

## Tests
- `Core.Tests/Effects/DamageFormulaTests` (about 205 lines after T1):
  - `TieredAmount_SheetLowAtPowerOneWithFourHundredSeventyThreePercent_IsEleven` (1 x 2 x 5.73 = 11.46).
  - `TieredAmount_SheetExtremeAtPowerFiveHundred_Is166170`.
  - `TieredAmount_ModifiersAtMinusOneHundredPercent_IsZero` (the floor; `Calculate` then gives the minimum 1).
- `Core.Tests/Statuses/StatusModifiersTests` (51 lines): `Apply_Weakened_LowersTheDamageModifier`,
  `Apply_TwoDamageDealtChanges_AddUp`.
- `Core.Tests/Battle/BattleWeakenedTests` (new; combatants and records built in the test, certain hits, no crit):
  - `Execute_WeakenedHoldersTieredSkill_DealsLess` (40 becomes 30).
  - `Execute_WeakenedHoldersAttack_DealsLess`.
  - `Execute_WeakenedHoldersHeal_HealsTheSame` (decision 786's heal side).
  - `Execute_WeakenedHoldersItem_DealsTheSame` (a bomb built in the test).
- `Data.Tests/Statuses/StatusCatalogValidatorTests`: rows (`damageDealtChange` -100; 0) and
  `Validate_DamageDealtChangeOfMinus99_Accepted`.
- `Data.Tests/Content/ContentCatalogLoaderStatusTests`: `Load_RepositoryEnfeeble_GrantsWeakened` (ids only).

## Verify by hand
- Run the game, Title > New game, choose Sarxian, fight the goblins, Skill > Enfeeble on one goblin (it costs
  nothing and builds Instability): a "W" mark named Weakened; that goblin's hits on Sarxian are about a quarter
  smaller than the others' for three of its turns.
