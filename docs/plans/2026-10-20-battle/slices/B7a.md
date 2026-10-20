# B7a. Victory: experience fill

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B7a. Victory: experience fill** (Game, about 270 lines)
  - **Goal:** after the victory's beats, the scene calls `FinishBattle`. A plain victory screen over the battle shows:
    - "Victory!";
    - the experience earned;
    - each standing member's name, level and bar filling level by level, with "Level Up!" (decision 103).
    Confirm skips to the end; a second Confirm returns `BattleFinished`.
  - **New types (Game):**
    - `Battle/ExperienceBarAnimation` (pure): one member's bar over time through their `LevelProgressStep`s; each step takes `max(0.2 s, 1.1 s x share of the level)`. `Skip()`, `IsComplete`.
    - `Battle/VictoryScreen` (pure part plus plain draw): its stages and the request it returns.
  - **Tests:**
    - Bar animation: `ExperienceBarAnimation_Update_TwoLevelUps_PassesThroughBothSteps`, `ExperienceBarAnimation_Skip_JumpsToFinalStep`, `ExperienceBarAnimation_NoSteps_IsComplete`, `ExperienceBarAnimation_SmallGain_TakesMinimumStepTime`.
    - Screen: `VictoryScreen_Update_ConfirmWhilePlaying_SkipsToEnd`, `VictoryScreen_Update_ConfirmWhenDone_RequestsBattleFinishedOnce`.
  - **Verify by hand:**
    - [ ] After the goblins, Sarxian gets 120 experience, and the Status page agrees.
