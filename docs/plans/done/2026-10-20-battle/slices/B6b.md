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
- **Done, with deviations:** about 350 lines, not 290. The skill and item lists share the panel's list drawing through `IBattleListChoice` (cursor position and the 5 visible rows), with each list's labels, amounts ("10 MP", "x3") and hints formatted once when it opens. `CommandMenu` and `BattleScene` now take the inventory and the catalog's items. A greyed item does nothing on Confirm (tested); the skill and item lists share one cursor-move helper. The scene test became the menu test `Update_ConfirmItemsAfterLastPotionUsed_NoLongerListsIt`, since the scene doesn't expose its menu; the menu rebuilds the list from the bag on each turn. Skills greyed for a member without skills was already done in B6a. `BattleSceneTests.DefendAndSettle` presses Down three times, now that the new game's potions enable Items.
