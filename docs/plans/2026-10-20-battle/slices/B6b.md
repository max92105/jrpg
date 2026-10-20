# B6b. Items in battle

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B6b. Items in battle** (Core + Game, about 290 lines)
  - **Goal:** Items lists the owned consumables in pickup order, with icon, name, "x3" and the item's description.
    - Items Core can't use now are greyed (the revive with nobody fallen).
    - Confirm aims as for skills, and using the last copy removes the row.
    - Skills is greyed for an actor without skills, and Items with nothing usable (decision 105).
  - **Core:** `BattleState.TargetChoiceFor(ItemDefinition item)` (throws without an effect). `CanUse(item)` is also false with no candidate.
  - **Tests:**
    - Core: `BattleState_TargetChoiceFor_ItemWithoutEffect_Throws`, `BattleState_CanUse_ReviveWithNobodyFallen_ReturnsFalse`, `BattleState_CanUse_ReviveWithAllyFallen_ReturnsTrue`.
    - Menu: `CommandMenu_ConfirmItems_ListsOwnedConsumablesWithCounts`, `CommandMenu_ReviveWithNobodyFallen_IsGreyed`, `CommandMenu_ActorWithoutSkills_SkillsGreyed`, `CommandMenu_NoUsableItem_ItemsGreyed`, `CommandMenu_ConfirmBombAll_ProducesCommandOnEveryLivingEnemy`.
    - Scene: `BattleScene_Update_UseLastPotion_ListNoLongerShowsIt`.
  - **Verify by hand:**
    - [ ] Potion, Bomb, Bomb All and Revive Feather work.
    - [ ] The menu's inventory matches after the battle.
