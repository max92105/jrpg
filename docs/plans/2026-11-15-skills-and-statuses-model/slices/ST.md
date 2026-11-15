# ST. A status acts at the start or the end of its holder's turn

Core + Data + content, about 240 lines (about 150 of them tests). Needs SW (TT for the content line only). Design:
README, decisions 804 and 805.

## Goal
Each status says when its tick acts: `"actsAt": "startOfTurn"` or `"endOfTurn"` (decision 695). A start tick runs as
its holder's turn starts, before a stun skips it, and never on a readied extra turn; a holder it fells loses the turn,
the next actor comes, and a battle it ends ends at once. The countdown stays at the end of the holder's turn (ADR
0021). Burning acts at the start; every other record says `endOfTurn`.

## New types
- `Core/Statuses/StatusTiming` (public enum): when a status's tick acts in its holder's turn. `StartOfTurn`,
  `EndOfTurn`.

## Changes
- `Core/Statuses/StatusDefinition`: `StatusTiming ActsAt { get; init; } = StatusTiming.EndOfTurn` ("decision 695;
  moves only the tick, never the countdown").
- `Core/Battle/TurnUpkeep.TickStatuses(Combatant holder, StatusTiming timing, IReadOnlyList<Combatant> combatants,
  List<BattleEvent> events)`: ticks the statuses acting at `timing` (four parameters, the limit).
- `Core/Battle/TurnFlow`: `RunUpkeep` ticks `EndOfTurn`; `AdvanceToNextActor` calls a new private
  `StartTurn(List<BattleEvent> events)` after each `turnOrder.Advance()`: on a turn that isn't a readied extra turn,
  `TickStatuses(current, StartOfTurn, ...)`; it returns the outcome when the tick ended the battle. The loop then
  advances again while the current actor has fallen or is stunned. The method's comment states the order (start tick,
  then the stun skip). Keep `AdvanceToNextActor` under CA1502 through `StartTurn`; a second helper
  `SkipsItsTurn(Combatant actor)` (fallen or stunned) if the loop's condition grows.
- `content/statuses.json`: every record gets `"actsAt"` (written always; `ActsAt` is declared between `IsHarmful` and
  `Parts`, so the file writes it after `"isHarmful"`);
  `burning_1` `"startOfTurn"`, the rest `"endOfTurn"` (decision 805).
- `Game`: no change expected; the playback test below checks that a start tick after the actor's events plays.

## Tests
- `Core.Tests/Battle/BattleTurnStartTickTests` (new: `BattleTickingStatusTests` is at about 210 after TT; records and
  combatants built in the test, turn order from a seed that puts the holder after the actor):
  - `Execute_StartOfTurnStatus_TicksWhenTheHoldersTurnStarts` (the tick's event follows the actor's `Execute`).
  - `Execute_StartOfTurnStatus_DoesNotTickAtTheEnd`.
  - `Execute_EndOfTurnStatus_StillTicksAtTheEnd` (the other side).
  - `Execute_StartTickFellsTheHolder_TurnPassesToTheNext` (no `ActionStarted` for the fallen; the next actor is
    current).
  - `Execute_StartTickFellsTheLastEnemy_EndsInVictory`.
  - `Execute_StartTickOnAStunnedHolder_TicksThenSkips` (events in that order).
  - `Execute_ReadiedExtraTurn_HasNoStartTick` (decision 202's extra turn).
  - `Execute_StartOfTurnStatus_CountsDownAtTheEnd` (granted for 1 turn: ticks once at the start, ends at the end).
- `Data.Tests/Content/ContentCatalogLoaderStatusTests` (from SC1): `Load_StatusWithUnknownTiming_RefusesNamingIt`.
- `Game.Tests/Battle/ActionPlaybackPhaseTests` (92 lines): `Start_TickOfTheNextActorAfterAnAttack_PlaysItsTickBeat`
  (an attack's events then a `StatusTicked` of another combatant).
- `ContentWriterRepositoryTests.Statuses_RepositoryFile_WritesBackUnchanged` keeps passing.

## Verify by hand
- Run the game, Title > New game, choose Omer, fight the goblins, Fire Blast one: the Burning tick shows as the
  goblin's turn starts, before it attacks; a goblin with 6 HP or less falls to it without attacking.
