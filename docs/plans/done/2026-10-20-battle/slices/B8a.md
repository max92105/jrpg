# B8a. Escaped

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B8a. Escaped** (Game, about 110 lines)
  - **Goal:** after Run, "Escaped" shows on the banner over a scrim for 1.5 s, or until Confirm, then the screen returns to the map.
  - **New types (Game):** `Battle/EscapedNotice` (pure timing).
  - **Tests:** `EscapedNotice_Update_PastDisplayTime_IsDone`, `EscapedNotice_Update_Confirm_IsDone`, `BattleScene_Update_RunThenWait_RequestsBattleFinished`.
  - **Verify by hand:**
    - [ ] Run shows the banner, then fades to the map.
- **Done, with deviations:** about 140 lines. `EscapedNotice` returns `BattleFinished` once, like `VictoryScreen`, rather than exposing `IsDone`. The scene draws the notice itself (`DrawEscaped`: scrim, full-width banner band across the middle, "Escaped" at 64 px), over the HUD as legacy's outcome layer did. Once a battle is finished, `BattleScene.Update` hands every update to the outcome screen in one place (which also kept its complexity under the limit). The Run tests now expect the notice first: `Update_ConfirmRun_ShowsEscapedAndClearsEncounter`, the planned `Update_RunThenWait_RequestsBattleFinished`, and `GameFlowTests.Update_WalkIntoForestThatAlwaysRolls_FightsAndReturnsToMap` waits the 1.5 s.
