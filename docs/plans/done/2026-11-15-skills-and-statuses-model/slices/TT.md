# TT. A tick may have a damage type: Burning meets fire resistance

Core + Data + content, about 150 lines (about 90 of them tests). Needs SW. Design: README, decisions 803 and 805;
design.md, section 2.3.

## Goal
A `damageEachTurn` part may name a `damageType`; its tick then meets the holder's resistance to that type (Guard's
raise included) through `DamageFormula.Calculate`, at least 1, and never Exposed, a crit, a block, penetration or a hit
trigger. An untyped tick ignores resistances as Poison does today. `burning_1` becomes fire: against the goblin's fire
weakness (-25 %) its 5 becomes 6.

## Changes
- `Core/Statuses/StatusPart`: `StatusPart(StatusPartKind Kind, int Amount = 0, DamageType? DamageType = null)`; the
  doc: "the type a damage tick meets resistance as; null ignores resistances (decision 803)".
- `Core/Battle/TurnUpkeep.TickStatuses`: a damage part with a type takes `DamageFormula.Calculate(amount,
  DamageFormula.NormalHit, new Mitigation(holder.CurrentStats.Resistances.Of(type), 0))`, through a private
  `TickDamageOf(Combatant holder, StatusPart part)`, so `TickStatuses` stays under CA1502; the summary says which ticks
  meet a resistance.
- `Data/Statuses/StatusCatalogValidator`: a `damageType` only on `damageEachTurn` (`"parts[0]: damageType is only for
  damageEachTurn (was fire on healEachTurn)."`).
- `Data/Content/AbsentWhenNullFilter`: `(typeof(StatusPart), "damageType")` ("Decision 803: only a typed tick names
  its type.").
- `content/statuses.json`: `burning_1`'s part gets `"damageType": "fire"` (decision 805), in the writer's layout.

## Tests
- `Core.Tests/Battle/BattleTickingStatusTests` (156 lines, about 210 after; records built in the test):
  - `EndTurn_FireTickOnFireWeakHolder_TakesMore` (-25 %: 8 becomes 10).
  - `EndTurn_FireTickOnGuardedHolder_TakesLess` (a Guard of 50 points on the holder: 8 becomes 4).
  - `EndTurn_TypedTickOnExposedHolder_IgnoresExposed`.
  - `EndTurn_TypedTickAgainstFullResistance_TakesOne` (the minimum).
  - `EndTurn_UntypedTickOnFireWeakHolder_TakesItsAmount` (the untyped side of decision 803).
- `Data.Tests/Statuses/StatusCatalogValidatorTests`: a row of `InvalidStatuses` (a type on `healEachTurn`) and
  `Validate_TypedDamageEachTurn_Accepted`.
- `ContentWriterRepositoryTests.Statuses_RepositoryFile_WritesBackUnchanged` keeps passing with the new field.

## Verify by hand
- Run the game, Title > New game, choose Omer, fight the goblins, Fire Blast one: at the end of its turn the Burning
  tick shows 6, not 5 (the goblin is weak to fire).

- **Done, with deviations:** the tick tests are named `Execute_...` rather than `EndTurn_...`, as every test in
  `BattleTickingStatusTests` names `BattleState.Execute`; they put the statuses on a goblin whose turn comes after the
  hero's Defend (`GoblinToAct`), since the hero's builder has no resistances to set. The full-resistance test ticks 3
  against a fire resistance of 100 %, held at the 75 % cap: 0.75, rounded down, raised to the minimum 1. Added beyond
  the spec: the family rule now compares each part's kind and type (decision 797's "kinds, types and stats"), with
  `Validate_OneFamilyWithAnotherDamageType_Throws`; and `ContentCatalogLoaderStatusTests`'
  `Load_RepositoryStatuses_BurningTicksAsFire` reads decision 805 off the repository's `burning_1` (kind and type, no
  number). Simulations: all 43 pass; the Goblin Chief's lines are unchanged from SW (level 1 alone: fighter 91.4 %,
  mage 16.6 %, cleric 51.4 %, rogue 52.8 %, barbarian 98.6 %, paladin 85.2 %, monk 78.6 %, warlock 24.2 %, druid
  43.2 %; a party of 4 wins 100 % at levels 1, 3 and 5). The hand check is the developer's to see.
