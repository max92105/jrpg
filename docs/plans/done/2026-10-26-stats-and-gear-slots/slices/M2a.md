# M2a. Test fixtures fold defense into attack

Part of [the stats and gear slots plan](../README.md). After M1, right before M2b, so M2b's removal of defense changes no battle expectation.

- **M2a. Test fixtures fold defense into attack** (tests, mechanical, about 140 lines)
  - **Goal:** every battle test that doesn't test defense itself fights with defense 0, its setup folding the defense it met into the attacker's numbers (review edit), so the expected values hold unchanged: a hero of attack 15 against a goblin of defense 2 becomes attack 13 against defense 0; a goblin of attack 20 against a member of defense 8 becomes attack 12; a skill or item of power p against defense 2 becomes power p − 2. No production code changes; the test count is the same before and after.
  - **Changes (tests only):**
    - `Builders/EnemyDefinitionBuilder`: `WithStats(maxHp, attack)`; `WithDefense(defense)` for the tests about defense, until M2b turns them into resistance tests.
    - `Builders/PartyMemberBuilder`: `WithStats(attack)` and `WithDefense(defense)` likewise; default defense 0.
    - Every caller (about 43 lines with `attack:`/`defense:` in 18 files, the enemy builder's 57 uses in 24 files): the defense argument goes and its value folds into the other side's attack or the effect's power, as above.
    - **Where folding can't hold** (one attacker meets two different defenses in one test), the expectation is re-pinned instead, with the reason in the commit ("defense leaves in M2b; raw power lands whole"). Candidates from a grep of the setups (M2a confirms each and lists it in the Done note):
      - `BattleStateOutcomeTests` (the `GrantVictoryExperience_*` tests) and `BattleStateTests` (`UpcomingActors_*`), split from `BattleStateTests` in C2: members of defense 8 beside the builder's default 5 in one battle, hit by the same goblin (attack 20);
      - `GameSessionBattleTests` (its goblin, attack 20, against the repository leader's class defense and a test member's 5);
      - a skill or item with one power hitting all enemies when the enemies were built with different defenses (`BattleSkillAndItemTests`, `BattleMultiHitTests`: check);
      - `BattleStatusTests` line 265 (a hero of defense 5 guarded by the paladin's aura): this one is about defense and keeps `WithDefense` until M2b.
    - A test whose meaning needs defense (Guarded's tests, `DamageFormulaTests`' defense cases) keeps it through `WithDefense` and is listed for M2b.
  - **Tests:** none new; record the solution's test count before and after (same), the folded setups by file, and every re-pinned assertion with its old and new value.
  - **Verify by hand:** none.
- **Done, with deviations:**
  - **Count:** 2,478 tests before and after.
  - **Builders:** every builder's defense defaults to 0: the enemy's (was 2), the member's and `ClassBuilder`'s (were 5). The member builder's attack is `WithAttack(attack)` (it wears a test weapon since M1a), not `WithStats(attack)`.
  - **Folded setups:**
    - `BattleStateTests`, `BattleStateOutcomeTests`, `BattleStateTargetTests`: the hero 15 → 13 and the goblin 20 → 12.
    - `BattleMultiHitTests`: Flurry 8 → 6, Palm 10 → 8.
    - `BattleStatusTests`: Bash 15 → 13, Jab and Quake 5 → 3, the Exposed hero 15 → 13.
    - `BattleTickingStatusTests`: Jab 5 → 3.
    - `BattleSkillAndItemTests`: Fireball 20 → 18.
    - `BattleQuickActionTests`: Dash 10 → 8.
    - Every other `WithStats(..., defense: n)` call just lost its defense; its test didn't depend on it.
  - **Re-pinned** (defense leaves in M2b, so raw power lands whole):
    - `BattleDominanceTests.Execute_DamageSkillWhileItsForceDominant_EffectPowerRaised`: 28 → 30. Defense came off after the +50 %, so a folded power would have been an odd 19.
    - `BattleDominanceTests.Execute_Equilibrium_SpendsBothForces`: 43 → 45, so its comment's "base 45" stays literal (a fold to power 43 was possible).
  - **Kept through `WithDefense`, for M2b:**
    - `PartyMemberStatTests`: 5 tests, summing worn defense.
    - `CombatantTests`: `FromPartyMember_EquippedMember_UsesTotalStatsOnPartySide` (5) and `FromEnemy_Definition_UsesItsStatsOnEnemySide` (2).
    - `BattleStatusTests.GuardingHero` (5), the two Guarded tests.
    - `BattleTickingStatusTests.Execute_PoisonedActorEndsTurn_LosesPowerIgnoringDefenseAndExposed` (5; from the review): poison ignoring defense needs a defense to ignore.
  - **The plan's other candidates:** none needed a re-pin. `GrantVictoryExperience_*` and `UpcomingActors_*` are covered by their file's fold. `GameSessionBattleTests` and the multi-target skills passed with the defense simply dropped.
  - **For M2b:** `BattlePowerTests` and `BattleResistanceTests` still say "defense 0" in their header comments; clean them up when defense leaves.
