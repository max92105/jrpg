# FM2. Move on screen

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "When things happen in a turn" (events table), "Replacement mapping" (positions), "Risks" (formation and layout). ADR 0023.

- **FM2. The Move command on screen; battlers stand where the formation says** (Game, about 240 lines)
  - **Goal:**
    - The command list gets Move, between Items and Defend. It opens the target cursor on the actor's neighbors; Confirm moves.
    - The banner says "Move". At the next refresh, both battlers stand in their new spots, and their panels follow their places in the line.
    - LK3 makes them slide.
  - **Changes (Game):**
    - `CommandEntry.Move`; `CommandMenu.Entries` gains it; `CommandMenu` opens `BattleTargetCursor` on `battle.TargetChoiceForMove()` and returns a `MoveCommand`.
    - `BattleLayout.SpotsOf(IReadOnlyList<Combatant> combatants, Formation formation)`: party spots from `formation.PositionOf`; enemies unchanged. `BattlersView` (and the target pointers) recompute spots when a stage starts, never per frame.
    - `PartyStatusView` orders its panels by the formation at each refresh.
    - `ActionPlayback.BannerTextFor` handles `MoveCommand`; `Read` handles `PlacesSwapped` (no extra beat: the banner is the beat).
    - `ui.json` and `MessageIds`: `battle_command_move` ("Move") with its hint line.
    - Tests: `CommandMenuTests` and `CommandPanelView`-related tests that walk the entry list pick entries by what they are (tests rule); check every test that counts entries.
  - **Tests:**
    - `CommandMenuTests`:
      - `Update_ConfirmMove_TargetsOnlyNeighbors`
      - `Update_ConfirmMoveTarget_ProducesMoveCommand`
      - `Update_CancelOnMoveTargets_ReturnsToCommands`
    - `BattleLayoutTests`: `SpotsOf_AfterSwap_MembersTradeSpots`
    - `ActionPlaybackTests`: `Start_MoveCommand_ShowsMoveBanner`
  - **Verify by hand:**
    - With three members, the middle one moves to the front: both battlers trade spots, the panels reorder, and the turn passes.
    - An end member is offered only one neighbor.
