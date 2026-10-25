# RG2. Quick skills keep the turn

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "When things happen in a turn" (step 3). Decision 202; question 214.

- **RG2. A quick skill doesn't end the actor's turn** (Core + Data, about 140 lines)
  - **Goal:**
    - A skill marked quick resolves, and the same actor acts again at once: no upkeep, no advance, `CurrentActor` and `Phase` unchanged.
    - A quick skill that wins or loses the battle ends it as usual.
    - Data requires a quick skill to spend a resource, since a free one would give endless turns.
    - RG3's Ready is the first quick skill.
  - **Changes:**
    - Core:
      - `Skills/SkillDefinition.IsQuick` (default false);
      - `BattleState.EndTurn` keeps the turn after a quick skill while the battle goes on, decided by a private `KeepsTurn(BattleCommand)`.
    - Data: `SkillCatalogValidator`: "a quick skill must spend a resource".
  - **Tests:**
    - `BattleQuickActionTests` (new file):
      - `Execute_QuickSkill_SameActorActsAgain`
      - `Execute_QuickSkill_NoUpkeepRuns`
      - `Execute_AttackAfterQuickSkill_TurnEnds`
      - `Execute_QuickSkillFellsLastEnemy_BattleEnds`
    - `SkillCatalogValidatorTests`: `Validate_QuickSkillWithoutCost_Throws`
  - **Verify by hand:** none (RG4 adds the first quick skill to content).
- **Done, with deviations:** Tests 1925 before, 1930 after. Beyond the plan:
  - The turn flow left `BattleState` for a new internal `Battle/TurnFlow`: upkeep, outcome, advance past the stunned, and the quick skill's `KeepTurn`. It returns the phase after. The 1925 existing tests passed unchanged across the move.
  - `BattleState` went from 319 to about 270 lines, and its backlog line is removed.
  - `Execute`'s command switch moved into `CarryOut` for the complexity limit.
  - From review: the no-upkeep test also checks that a held status isn't counted down after a quick skill.
