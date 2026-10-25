# WL1. The warlock: casting builds instability

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (warlock rows), "Key design decisions" 6. Decisions 194, 203; question 216.

- **WL1. The warlock: casting builds instability, and stronger spells need more** (content + Game + tests, about 150 lines)
  - **Goal:**
    - Sarxian has Instability (max 10, starts at 0, −1 at the end of each of his turns).
    - Eldritch Bolt adds 2. Void Rift adds 3 and needs at least 4. Chaos Lance adds 4 and needs at least 7.
    - A gain past the max is allowed; the amount stops at the max.
    - The skill list reads "+2 Instability", or "Needs 4 Instability" while short.
    - The danger is WL2.
  - **Changes:**
    - Core: nothing new to build. Positive changes (M2a) and requirements (CL1) exist; a negative per-turn gain is already allowed. One test pins that requirements are judged before the skill's own gain.
    - Data: none.
    - Content:
      - `classes.json`: the warlock gets `instability` (`perTurn` −1) and `skillIds` `eldritch_bolt`, `void_rift`, `chaos_lance`;
      - `skills.json`: the three skills.
    - Game:
      - `SkillCostText` writes a gain as "+{amount} {resource}";
      - `ui.json` and `MessageIds`: `battle_skill_resource_gain`.
    - Tests: any Game test that casts Sarxian's first skill with a fixed seed now casts Eldritch Bolt; check the expected numbers.
  - **Tests:**
    - `BattleStateResourceTests`:
      - `Execute_WarlockTurnEnds_InstabilityFallsByOne`
      - `CanUse_RequirementJudgedBeforeOwnGain_StillShortAtThree`
      - `Execute_GainPastMax_StopsAtMax`
    - `SkillCostTextTests`:
      - `CostOf_GainSkill_SaysPlusAmount`
      - `CostOf_GainAndRequirement_ShowsBoth`
    - `ContentCatalogLoaderTests`: `Load_RepositoryContent_WarlockHasInstability`
  - **Verify by hand:** as Sarxian against PT's dummy (revert the forest edit after):
    - Void Rift and Chaos Lance start with "Needs ...".
    - Each Bolt nets +1 over his turn, so Void Rift unlocks on his fifth turn.
    - Instability drains when he stops casting.
