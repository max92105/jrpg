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
