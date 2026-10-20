# B8b. Defeat

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B8b. Defeat** (Game, about 290 lines; decision 65)
  - **Goal:** after the defeat's beats, or at once for a battle lost at the start, the legacy `DefeatScreen` shows a red "Defeat", "The party has fallen.", and three choices on pointed bars:
    - **Load last save** (selected first): loads `SaveSlots.MostRecent` at once.
    - **Load:** opens the save list over the screen; Cancel closes it.
    - **Main menu:** ends the session.
    With no readable save, both load entries are greyed and the cursor starts on Main menu.
  - **New types (Game):**
    - `Battle/DefeatEntry`: LoadLastSave, Load, MainMenu.
    - `Battle/DefeatMenu` (pure): the entries and cursor.
    - `Battle/DefeatScreen`: draws the screen and hosts `Saving/SaveFilesPanel`.
      - It returns `LoadRequested(save)` or `TitleRequested`, once.
      - `BattleScene` takes `SaveSlots` (fourth parameter).
  - **Tests:**
    - Menu: `DefeatMenu_Start_SelectsLoadLastSave`, `DefeatMenu_NoReadableSave_BothLoadsGreyedAndMainMenuSelected`.
    - Screen: `DefeatScreen_Update_ConfirmLoadLastSave_RequestsMostRecent`, `DefeatScreen_Update_ChooseFileFromLoad_RequestsLoadingIt`, `DefeatScreen_Update_ConfirmMainMenu_RequestsTitle`.
    - Scene: `BattleScene_Update_BattleLostAtStart_ShowsDefeat`.
    - Flow: `GameFlow_Update_DefeatThenMainMenu_EndsSession`.
  - **Verify by hand:**
    - [ ] Lose with a near-dead leader: Load last save restores the latest save.
    - [ ] Load picks another file.
    - [ ] Main menu ends the game.
    - [ ] With no saves, both loads are greyed.
    - [ ] Leave only fallen members active on the Party page, walk into a battle: Defeat.
