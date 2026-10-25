# RS3. A rest item

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Rest item). Decision 198; question 221.

- **RS3. A rest item, used from the menu** (Core + Data + content + Game, about 200 lines)
  - **Goal:**
    - A "Camp Kit" used from the item menu rests the whole party, as the inn does, and is consumed.
    - It can't be used in battle.
    - The starting inventory gets one, so it can be tried at once (placeholder).
  - **Changes:**
    - Core:
      - `Effects/EffectKind.Rest`;
      - `Party/FieldItemUse`: a `Rest` item is usable while any member has something to restore, and using it calls `PartyMember.Rest()` on every recruited member;
      - `BattleState.CanUse(item)` is false for `Rest`;
      - `EffectResolver` throws for `Rest`, since it never reaches it: the comment says why.
    - Data: `ItemCatalogValidator`: a `rest` item is usable from the menu and targets all allies.
    - Content:
      - `items.json`: `camp_kit` ("Camp Kit", "Rests the party.", `Consumable.png`, stack 9);
      - `new-game.json`: one in the starting items.
    - Game: `ItemUseDialog` uses a `Rest` item at once, with no member to pick, and shows "The party rests." (reuses `event_party_rests`).
  - **Tests:**
    - `FieldItemUseTests`:
      - `Use_CampKit_RestsEveryMemberAndConsumes`
      - `CanUse_CampKitNothingToRestore_ReturnsFalse`
    - `BattleSkillAndItemTests`: `CanUse_CampKitInBattle_ReturnsFalse`
    - `ItemCatalogValidatorTests`: `Validate_RestItemNotUsableFromMenu_Throws`
    - `ItemUseDialogTests`: `Confirm_CampKit_UsesWithoutPickingMember`
  - **Verify by hand:** wear Omer down, open Items, use the Camp Kit: the count drops, and the next fight starts at 3 mana with full HP.
