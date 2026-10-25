# PL2b. The aura's range in the formation

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (paladin skills), "Content format" (`alliesInRange`). Decision 201; questions 210, 211. ADR 0023.

- **PL2b. The paladin's aura reaches the allies within its range in the formation** (Core + Data + content + Game, about 250 lines)
  - **Goal:**
    - An effect aimed at "allies in range N" reaches the actor and every living ally within N places along the formation.
    - Aura of Protection (4 conviction, range 1) gives Guarded +5 for 3 turns to the paladin and the allies beside him.
    - Wide Aura (6 conviction, range 2) gives +3 to two places on each side.
    - The aura applies at the cast (question 210), so the paladin moves (FM1) to cover who needs it.
    - The target cursor shows every ally the aura will reach before Confirm.
  - **Changes:**
    - Core:
      - `Effects/Targeting.AlliesInRange`;
      - `Effects/Effect` gains a defaulted `int Range = 0`;
      - `TargetRules.HitsEveryValidTarget` includes `AlliesInRange`;
      - `BattleState.ChoiceFor` takes the effect rather than only its targeting (a private overload). For `AlliesInRange`, the candidates are `Formation.WithinRange(actor, effect.Range)` still standing; every other targeting is unchanged.
    - Data: `EffectValidator`: `range` ≥ 0, and only on `alliesInRange`. An item can't use `alliesInRange`, since an item has no formation outside battle; `ItemCatalogValidator` says so.
    - Content:
      - `classes.json`: the paladin's `skillIds` gains `aura_of_protection` and `wide_aura`;
      - `skills.json`: the two skills.
    - Game: `BattleTargetCursor` already shows all candidates for an all-target choice; check it does for `AlliesInRange` too.
  - **Tests:**
    - `BattleFormationTests`:
      - `TargetChoiceFor_AuraRangeOneInMiddle_ReachesThree`
      - `TargetChoiceFor_AuraRangeOneAtEnd_ReachesTwo`
      - `TargetChoiceFor_AuraSkipsFallenInRange`
      - `TargetChoiceFor_AfterMove_RangeFollowsNewPlace`
      - `Execute_Aura_GuardsOnlyAlliesInRange`
    - `EffectValidatorTests`: `FindProblem_RangeOnOtherTargeting_ReturnsProblem`
    - `ItemCatalogValidatorTests`: `Validate_ItemWithAlliesInRange_Throws`
  - **Verify by hand:**
    - With five members, Teelle in the middle: Aura guards three ("GUARD 3"), and Wide Aura all five.
    - Move Teelle to the end and cast Aura: two guarded.
- **Done, with deviations:** Tests 1906 before, 1914 after. Beyond the plan:
  - `TargetRules` treats `AlliesInRange` as "every living ally" and as hitting every valid target. `BattleState.ChoiceFor(Effect)` then narrows it to `Formation.WithinRange`, so the legality check for a cast reads the same narrowed list.
  - `BattleTargetCursor` needed no change: it already shows every candidate of a takes-all choice.
  - Both skills' descriptions are marked as placeholders.
  - From review: a negative range and a range on another targeting get their own messages, and `FindProblem_NegativeRange_ReturnsProblem` covers the first.
