# C1a. The nine classes, with descriptions; items lose their class restriction

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (classes) and "Replacement mapping".

- **C1a. The nine classes, with descriptions; items lose their class restriction** (Core + Data + content + Game, about 220 lines, part of it deletions; decisions 162, 164, 165, 167)
  - **Goal:**
    - The game's classes are the nine of decision 162, each with a placeholder description: Warrior, Mage, Cleric, Rogue, Barbarian, Paladin, Monk, Warlock, Druid.
    - Existing members take their decided classes.
    - Any member can wear any equipment.
  - **Changes:**
    - Core:
      - `Party/ClassDefinition`: `(string Id, string Name, string Description)`. The summary says what a class is (ADR 0017), and C1b adds the stats.
      - `Items/EquipmentStats` loses `AllowedClassIds` and `CanBeWornBy`.
      - `Party/PartyEquipment`'s two class checks go. Its summaries no longer say "the member's class can wear it".
    - Data:
      - `Party/ClassCatalogValidator`: `description must not be empty.`
      - `Items/ItemCatalogValidator`: its two `allowedClassIds` rules go. If it no longer needs the class ids, drop that parameter and its callers' argument.
      - `Saving/SaveGameRepair.RepairWornItem`: the class condition goes; slot and existence checks stay.
    - Content:
      - `classes.json`: the nine, with one-sentence descriptions;
      - `party-members.json` `classId`: sarxian warlock, jazog barbarian, zanyr cleric, zaruk monk, teelle paladin, thaali druid, kamilia paladin (until C2 removes her);
      - `items.json`: every `allowedClassIds` removed.
    - Game: anything that showed "can't wear" by class (`EquipmentChoice` filtering, hint texts) follows Core. Delete messages left unused (`MessageIdsTests` tells).
    - Tests:
      - Delete the class-restriction tests in Core, Data and Game (`CanBeWornBy`, unknown class id on an item, the equipment choice hiding an item by class, the save repair returning an item by class), and record their names in the Done note.
      - `ClassCatalogValidatorTests` build classes with a description.
  - **Tests:**
    - `ClassCatalogValidatorTests.Validate_EmptyDescription_ThrowsNamingTheField`
    - `PartyEquipmentTests.Equip_AnyClass_CanWearAnyItem` (a mage wears the iron sword)
    - `ContentCatalogLoaderTests.Load_RepositoryContent_HasTheNineClasses` (asserts the nine ids, not a total)
  - **Verify by hand:** the Status page shows Sarxian as "Warlock". The Equipment page offers him the swords and the robe alike.
