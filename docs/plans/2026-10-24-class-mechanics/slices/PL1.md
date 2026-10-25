# PL1. The paladin: conviction

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (paladin and cleric rows). Decision 192; questions 212, 217.

- **PL1. The paladin: conviction from fallen enemies and heavy hits; Smite; the cleric loses faith on heavy hits** (Core + Data + content, about 230 lines)
  - **Goal:**
    - Teelle has Conviction (max 10, starts at 0).
      - She gains 3 whenever an enemy falls, by anyone's hand, while she stands.
      - She gains 2 more when a single hit takes at least 20 % of her max HP.
    - Smite (3 conviction) is a strong blow. The aura comes in PL2b, once the formation exists.
    - The cleric's faith also drops by 1 on a heavy hit (question 217), which is content only once the trigger exists.
  - **New types:**
    - Core `Resources/HeavyHitRule` (public static): `IsHeavy(int hpLost, int maxHp, int thresholdPercent)`. It compares hpLost × 100 ≥ maxHp × threshold in integers, so nothing rounds. The comment cites decision 192.
  - **Changes:**
    - Core:
      - `Resources/ResourceGains`: `PerHeavyHit`, `HeavyHitPercent` and `PerEnemyFallen`;
      - `Battle/ResourceTriggers`: `OnHitTaken` adds the heavy-hit gain, and `OnCombatantFallen` gives every living party member their `PerEnemyFallen` when an enemy falls.
    - Data: `ResourceDefinitionValidator`: `heavyHitPercent` is 1..100 whenever `perHeavyHit` is set.
    - Content:
      - `classes.json`: the paladin gets `conviction` and `skillIds` `smite`; the cleric's `faith` gets `perHeavyHit` −1 and `heavyHitPercent` 20;
      - `skills.json`: `smite`.
  - **Tests:**
    - `HeavyHitRuleTests`:
      - `IsHeavy_ExactlyThreshold_ReturnsTrue`
      - `IsHeavy_JustBelow_ReturnsFalse`
    - `ResourceTriggersTests`:
      - `OnHitTaken_HeavyHit_GainsBothHitAndHeavyGains`
      - `OnHitTaken_HeavyHitOnCleric_LosesFaith`
      - `OnCombatantFallen_EnemyFalls_LivingPaladinGains`
      - `OnCombatantFallen_EnemyFalls_FallenPaladinGainsNothing`
  - **Verify by hand:**
    - As Teelle: +3 conviction per fallen goblin; Smite greys until 3.
    - Heavy hits: no goblin or dummy hit reaches 20 % at level 1, so the tests carry the rule. To see it on screen, set the paladin's `heavyHitPercent` to 3 locally for one run, check +2 more per hit, and revert it before committing (`git checkout content/classes.json`).
