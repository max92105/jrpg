# ST2. A stunned combatant loses its turn

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "When things happen in a turn" (step 6), "Key design decisions" 3, "Risks" (event order, `BattleState` grows). ADR 0021.

- **ST2. A stunned combatant loses its turn** (Core, about 150 lines)
  - **Goal:**
    - When the turn order reaches a stunned combatant, `Execute` skips that turn (`TurnSkipped`) and runs that combatant's upkeep, so the stun counts down.
    - It then checks the outcome and advances again, as often as needed. The UI never sees a stunned `CurrentActor`.
    - **Known gap:** shown from FT1.
  - **New types:**
    - Core `Battle/TurnSkipped(Combatant Combatant, StatusKind Cause)` (BattleEvent): a combatant lost its turn.
  - **Changes:** `Battle/BattleState.EndTurn`: the advance moves into a private `AdvanceToNextActor(List<BattleEvent>)`, which loops while the new actor is stunned. This keeps `EndTurn` under the complexity limit. The constructor is unchanged: nobody has a status at the start.
  - **Tests:**
    - `BattleStatusTests`:
      - `Execute_NextActorStunned_SkipsTurnAndReportsIt`
      - `Execute_StunForOneTurn_EndsOnTheSkippedTurn`
      - `Execute_TwoStunnedInARow_SkipsBoth`
      - `Execute_StunnedSkipped_CurrentActorIsTheOneAfter`
      - `Execute_OnlyEnemyStunnedBeforeMember_PhaseIsPartyTurn`
      - `Execute_StunnedActorsUpkeep_GainsPerTurn` (a lost turn is still a turn)
  - **Verify by hand:** none (no content yet).
- **Done, with deviations:** Tests 1848 before, 1853 after: 6 added, 1 deleted. Beyond the plan:
  - `EndTurn` is split into `RunUpkeep`, `SettleOutcome` and `AdvanceToNextActor`, so the skipped turn reuses the upkeep and the outcome check.
  - Three ST1 tests had the stunned goblin act. They now follow the skip:
    - the fall test drops the goblin's Defend;
    - the countdown test uses three combatants, so a turn in between can be shown not to count;
    - `Execute_StatusRunsOut_ReportsStatusEnded` is deleted. `Execute_StunForOneTurn_EndsOnTheSkippedTurn` covers the same event, since a stun can only run out on a skipped turn.
  - `BattleState` is now 296 lines; the next slice that touches it should move turn flow out.
