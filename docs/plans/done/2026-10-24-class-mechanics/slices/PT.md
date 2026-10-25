# PT. A sturdy practice enemy

Part of [the class mechanics plan](../README.md). Question 226. The first build slice, so every later hand check can use a long fight.

- **PT. A sturdy practice enemy, in an encounter of its own** (content + tests, about 40 lines)
  - **Goal:**
    - The goblin trio falls in a few turns, before a resource can build up.
    - A placeholder enemy with a lot of HP lets the developer watch a resource over ten turns or more.
    - It sits in its own encounter, which no map uses, so no test that rolls the forest's encounters changes.
    - To fight it, the developer adds `training_dummy` to the forest's encounter list in the editor's map settings for the session, and reverts the edit before committing anything.
  - **Changes:**
    - Content:
      - `enemies.json`: `training_dummy`, "Training Dummy": 400 HP, 0 MP until MP3b removes the field, attack 20, defense 0, 10 experience. It reuses `Goblin.png` (portrait and battler, exact case) and `AttackEnemy.png`. Attack 20 lets even the mage (150 HP, defense 4) last about nine of its hits. That's still under 20 % of any class's HP, so heavy hits (question 212) are seen with a temporary local threshold edit, reverted after (PL1).
      - `encounters.json`: `training_dummy`, with that one enemy and `Forest.png`.
  - **Tests:** `ContentCatalogLoaderTests`: `Load_RepositoryContent_HasTrainingDummyEncounter`. The startup art check already covers the reused file names.
  - **Verify by hand:** in the editor, add `training_dummy` to the forest's encounters and save, then play: a long fight against one dummy. Revert the forest edit (`git checkout content/maps/forest.json`).
- **Done, with deviations:** as planned. The test also checks that no map's random encounters roll the dummy, so it can't reach a forest test by accident. Tests: 1781 before, 1782 after.
