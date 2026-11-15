# TN1. Enemies act in place; the party still steps

Game, about 70 lines (about 40 of them tests). Decision 643; tunables "Enemy pause", "Party step".

## Goal
On an enemy's turn the enemy never moves: its StepForward and StepBack stages still last 0.2 s each (the pause after the
turn order switches to it, and after its action), but it stays on its formation spot. A party member steps 50 px left
and back exactly as today.

## Changes
- `Game/Battle/BattleTurnSequence.StepShare`: 0 for an enemy, whatever the stage; its summary says "a party actor".
- `Game/Battle/BattlersView.SteppedSpot`: only the party steps, leftward (the side ternary goes; the comment names
  decision 643). The class summary's "the actor stepped out toward the other side" becomes "a party actor stepped out
  toward the enemies".
- `BattlePacing.StepDistance`'s summary: "How far a party member steps out of the formation".
- Grep "steps toward", "step forward" and "stepped" in `Game/Battle/` comments and fix the ones that say enemies step.

## Tests
- `BattleTurnSequenceTests` (221 lines, re-pins only):
  - `Update_EnemyTurn_StepsForwardBeforeExecuting` becomes `Update_EnemyTurn_PausesInPlaceBeforeExecuting`: still
    StepForward at 0.1 s, and `StepShare` 0.
  - `Update_LongDelta_CarriesIntoNextStage`: the carried 0.05 s can't show as the second goblin's step share any more;
    it shows as the stage ending early: after the 1.45 s update, an update of 0.15 s less one tick keeps StepForward and
    one more tick ends it.
- `BattleTurnSequenceTests: StepShare_EnemyMidPause_IsZero` (an enemy's turn, 0.1 s in) and
  `StepShare_PartyMemberMidStep_IsHalf` (Sarxian, 0.1 s in: both sides of the rule).

## Verify by hand
1. `dotnet run --project src/Jrpg.Game`, load a save or start a new game, walk into the forest until a battle starts.
2. On a goblin's turn it doesn't move: the big diamond switches, a short pause, the attack, a short pause, the next
   turn.
3. On a party member's turn they step left toward the goblins and back, as before.

- **Done, with deviations:** no separate `StepShare_EnemyMidPause_IsZero`: the re-pinned `Update_EnemyTurn_PausesInPlaceBeforeExecuting` asserts the enemy's 0 at 0.1 s; `StepShare_PartyMemberMidStep_IsHalf` is the other side. `TurnStage`'s two step summaries say an enemy pauses in place.
  TN1's review: `BattleTurnSequence`'s summary (rewritten with TN2's wind-up) and `BattlersView.SpotOf`'s say only the party steps.
