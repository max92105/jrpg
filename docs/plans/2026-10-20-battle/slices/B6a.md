# B6a. Skills in battle

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B6a. Skills in battle** (Core + Data + Game, about 290 lines)
  - **Goal:** Skills opens the actor's skills in place of the commands. Each row shows:
    - the icon (none when `iconFile` is empty);
    - the name and "10 MP";
    - the skill's description as the hint.
    An unaffordable skill is greyed, and Confirm on it says "Not enough MP." (decision 105). Confirm opens targets for the skill's targeting:
    - "self" goes to the actor;
    - "all" selects everyone.
    Cancel goes back.
  - **Core:** `BattleState.TargetChoiceFor(SkillDefinition skill)` and `CanAfford(SkillDefinition skill)`. `CanUse(skill)` is also false with no candidate.
  - **Data:** `ArtReferenceCheck` covers skill icons that aren't empty (exact names).
  - **Tests:**
    - Core: `BattleState_TargetChoiceFor_AllEnemiesSkill_TakesAll`, `BattleState_TargetChoiceFor_SelfSkill_ListsOnlyActor`, `BattleState_CanAfford_NotEnoughMp_ReturnsFalse`.
    - Menu: `CommandMenu_ConfirmSkills_ListsActorSkills`, `CommandMenu_UnaffordableSkill_IsGreyedAndSaysNotEnoughMp`, `CommandMenu_ConfirmSelfSkill_ProducesCommandOnActor`, `CommandMenu_AllAlliesSkill_ProducesCommandOnEveryLivingMember`, `CommandMenu_CancelOnSkillList_ReturnsToCommands`.
  - **Verify by hand:**
    - [ ] All five skills work, and MP drops by the cost.
    - [ ] With MP spent (set in a save), skills are greyed.
- **Done, with deviations:** about 410 lines, not 290: the panel view's rows were split into one frame-and-backs pass plus a `CommandRowLabel` per row so commands and skills share them, and the menu tests moved Down presses past the now-enabled Skills (two `BattleSceneTests` too). A self skill opens the target pointer on the actor, as for any single target, rather than acting at once. Skills is greyed for a member who knows none; a skill with the MP but nobody to aim at is greyed and Confirm does nothing. "Not enough MP." replaces the hint until the selection moves. The list shows 5 rows and scrolls past that. Extra tests: `CanUse_ReviveWithNobodyFallen_ReturnsFalse`, `IsEnabled_ActorWithoutSkills_GreysSkills`, `Update_CancelOnSkillTargets_ReturnsToSkillList`, `FindProblems_SkillIconNameDiffersInCase_NamesTheSkill`.
