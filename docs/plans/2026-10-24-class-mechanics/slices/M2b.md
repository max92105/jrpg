# M2b. The mage's spells cost mana

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (mage skills), "Replacement mapping" (skill list, cannot-afford line). Decision 189.

- **M2b. The mage's spells cost mana; the skill list shows it** (content + Game, about 220 lines)
  - **Goal:**
    - Fire Blast costs 2 mana, Fireball 5, and the new Meteor 9 (power 60). With 3 mana at the start and +2 a turn, Meteor takes three turns of saving.
    - The skill list shows "2 Mana"; Confirm on an unaffordable spell says "Not enough Mana".
    - The mage's spells drop to 0 MP now; MP1 then removes the field everywhere. The cleric's heals still cost MP and say "Not enough MP" until MP1.
  - **New types:**
    - Game `Battle/SkillCostText` (internal sealed): a skill's cost line ("8 MP", "2 Mana") and its "not enough" line, formatted once per opened list.
  - **Changes:**
    - Content:
      - `skills.json`: `fire_blast` and `fireball` get their mana costs and `mpCost` 0, and the new `meteor` is added;
      - `classes.json`: the mage's `skillIds` gains `meteor`.
    - Game:
      - `BattleSkillChoice`: `IsSayingNotEnoughMp`/`SayNotEnoughMp` become `IsSayingCannotAfford`/`SayCannotAfford`;
      - `CommandMenu` uses the new names;
      - `CommandPanelView` takes amounts and the "not enough" line from `SkillCostText`;
      - `ui.json` and `MessageIds`: `battle_skill_resource_cost` ("{cost} {resource}") and `battle_not_enough_resource` ("Not enough {resource}.").
    - Tests: `CommandMenuTests.Update_UnaffordableSkill_IsGreyedAndSaysNotEnoughMp` keeps its meaning with a cleric at 0 MP.
  - **Tests:**
    - `SkillCostTextTests`:
      - `CostOf_MpSkill_SaysMp`
      - `CostOf_ResourceSkill_SaysAmountAndResourceName`
      - `CannotAffordText_ResourceShort_NamesResource`
      - `CannotAffordText_OnlyMpShort_SaysMp`
    - `CommandMenuTests`: `Update_ConfirmMeteorWithThreeMana_SaysNotEnoughMana`
    - `ContentCatalogLoaderTests`: `Load_RepositoryContent_MeteorCostsNineMana`
  - **Verify by hand:** as Omer against PT's dummy (forest edit reverted after):
    - Fire Blast on turn 1;
    - Meteor greyed with "Not enough Mana" until 9, then a big hit;
    - the next fight starts at 3 mana.
- **Done, with deviations:** Tests 1820 before, 1826 after. Three things differ from the plan:
  - `CommandMenuTests`' MP case already fought as Zanyr, a cleric (CS1), so only its names changed.
  - `BattleSkillChoice` also exposes `Actor` and `LackingResourceAt`, captured when the list opens, so the panel formats every row's "not enough" line then.
  - Meteor's description is marked as a placeholder.
