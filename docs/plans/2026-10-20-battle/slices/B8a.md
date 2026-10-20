# B8a. Escaped

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B8a. Escaped** (Game, about 110 lines)
  - **Goal:** after Run, "Escaped" shows on the banner over a scrim for 1.5 s, or until Confirm, then the screen returns to the map.
  - **New types (Game):** `Battle/EscapedNotice` (pure timing).
  - **Tests:** `EscapedNotice_Update_PastDisplayTime_IsDone`, `EscapedNotice_Update_Confirm_IsDone`, `BattleScene_Update_RunThenWait_RequestsBattleFinished`.
  - **Verify by hand:**
    - [ ] Run shows the banner, then fades to the map.
