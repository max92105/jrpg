# B5a. Turn pacing: step forward and back

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B5a. Turn pacing: step forward and back** (Game, about 270 lines)
  - **Goal:** turns take time as in legacy.
    1. The actor steps 50 px forward over 0.2 s.
    2. A party member waits for a command; an enemy acts.
    3. `Execute` runs, and the screen holds 1.0 s (2.0 s with a fall). Defend and Run don't hold.
    4. The actor steps back over 0.2 s.
    Input is ignored outside "awaiting a command". In B5b, the hold becomes the beats' length.
  - **New types (Game):**
    - `Battle/TurnStage`: StepForward, AwaitingCommand, Holding, StepBack, Over.
    - `Battle/BattlePacing`: the battle's durations.
  - **Changes:**
    - `BattleTurnSequence` gains `Update(TimeSpan delta)`, `Stage`, `StepShare(Combatant combatant)`. Leftover time carries into the next stage.
    - `Actor` stays the one who acted until the next StepForward.
    - `BattleScene` refreshes the turn order when StepForward starts, and the vitals text when Holding starts. Everything on screen keys off `Actor`, never `battle.CurrentActor`.
  - **Tests:**
    - Stages: `BattleTurnSequence_Update_EnemyTurn_StepsForwardBeforeExecuting`, `BattleTurnSequence_Update_PartyTurn_AwaitsCommandOnceForward`, `BattleTurnSequence_Submit_HoldsThenStepsBack`.
    - Holding: `BattleTurnSequence_Update_TargetFell_HoldsLonger`, `BattleTurnSequence_Update_Holding_ActorIsStillTheOneWhoActed`.
    - Timing: `BattleTurnSequence_Update_LongDelta_CarriesIntoNextStage`.
    - B1b's sequence tests gain deltas, assertions unchanged.
  - **Verify by hand:**
    - [ ] Step, pause, step back.
    - [ ] HP numbers change only once the action lands.
