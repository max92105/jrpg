# MO1. The monk: multi-hit skills and chakra

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (chakra row, monk skills), "When things happen in a turn" (step 2). Decision 206; question 218.

- **MO1. The monk: multi-hit skills and chakra from each hit landed** (Core + Data + content + Game, about 270 lines)
  - **Goal:**
    - A skill's effect can hit several times: each hit is resolved and reported on its own, and hits stop on a target once it falls.
    - Zaruk has Chakra (max 10, starts at 0) and gains 1 for each of his hits that lands, attacks included.
    - Palm Strike is a free 2-hit blow; Flurry spends 3 chakra for 4 hits of power 8.
    - On screen, each hit pops its own number at the impact.
  - **Changes:**
    - Core:
      - `Effects/Effect` gains a defaulted `int Hits = 1`;
      - `Resources/ResourceGains.PerHitLanded`;
      - `BattleState.ApplyToTargets` loops the hits per target, each reporting `EffectApplied` and running `ResourceTriggers.OnHitTaken` and a new `ResourceTriggers.OnHitLanded(Combatant actor, int hpLost, List<BattleEvent> events)`. The status grant still happens once per target, after the hits.
    - Data: `EffectValidator`: `hits` ≥ 1, and above 1 only for `damage`.
    - Content:
      - `classes.json`: the monk gets `chakra` and `skillIds` `palm_strike`, `flurry`;
      - `skills.json`: the two skills.
    - Game: `FloatingNumbersView` staggers numbers for the same target, so four hits read as four (a small rise offset per index, computed when the playback starts). `ActionPlayback` already makes one impact for many `EffectApplied`.
  - **Tests:**
    - `BattleMultiHitTests` (new file):
      - `Execute_FourHits_ReportsFourEffectApplied`
      - `Execute_TargetFallsMidway_StopsHitting`
      - `Execute_MonkHitsLand_GainsChakraPerHit`
      - `Execute_AttackLands_GainsOneChakra`
      - `Execute_MultiHitOnBarbarian_FerocityPerHit`
    - `EffectValidatorTests`: `FindProblem_HitsOnHeal_ReturnsProblem`
    - `ActionPlaybackTests`: `Start_FourHitsOneTarget_FourNumbersStaggered`
  - **Verify by hand:** as Zaruk against the dummy: attacks build chakra by 1; Flurry shows four rising numbers and spends 3.
- **Done, with deviations:**
  - The hits loop in `EffectLanding` (the landing left `BattleState` in an earlier slice). It is now a class built with the combatants, so `Land(actor, effect, targets, events)` stays at 4 parameters; `Fall` stays static.
  - A stack of numbers is `FloatingNumber.Stack`, set by `ActionPlayback`: each number popping on the same target at the same moment sits 30 px above the one before.
  - Extra tests: `effect.hits` of 0 is rejected.
  - `CommandMenuTests.IsEnabled_ActorWithoutSkills_GreysSkills` builds Zaruk with his class's skill list emptied, since every class in content now has skills.
  - After review: a multi-hit plays its attack strip once per target, and a test pins that a status is granted once, after the hits.
