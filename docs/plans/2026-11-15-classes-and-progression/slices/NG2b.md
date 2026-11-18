# NG2b. The chosen-row lists' shared piece, and the new game's leader choices

Editor, about 320 lines (about 200 of them tests). Needs NG2 (`NewGame/NewGameViewModel`, its fixture, the step).
Split from NG2 on 2026-11-17 (NG2's estimate passed 320 lines with its tests), and its starting items from it into
NG2c at its start (the extraction the review asked for made it pass 320 again). Design: sections 1, 3; decisions 968,
969; the mode bar's WS12 recipe, steps 1 and 7.

## Goal
`NewGameViewModel` types its leader choices, which NG2 kept as the record's: the members in the order the leader screen
shows them, picked by name from the draft's members not listed, added after the chosen row, moved up and down, removed
(at least one: Data's rule, shown on the line). `ToNewGame` keeps the record's list while the rows match it in order,
so an untouched new game still equals its record (decision 1014's rule for the class skills). Every change of the list
raises `Problem`'s change once.

The leaders and the starting items are the third and fourth chosen-row lists after a class's skills and resources (the
backlog's line, CL7d review): the move greying and the row watching are extracted first, the Skills and Resources tests
unchanged as the guard.

## Changes
- `Events/ListOrder`: `CanMoveUp` and `CanMoveDown` (a row chosen and not first or last), the greying every
  chosen-row list's Up and Down read.
- `Catalogs/ListItemWatch`: `WatchRowsOf(rows, watch, unwatch, listChanged)`: the rows held now, the ones the list
  gains and loses, then the owner's list-change step.
- `Party/ClassViewModel.Skills` and `.Resources` read both.
- `NewGame/NewGameViewModel.Leaders` (new partial): `ObservableCollection<RecordChoice> Leaders` (a member the draft
  lacks shown as `RecordChoice.Missing`), `SelectedLeader`, `LeaderToAdd`, `LeadersToAdd` (members not listed, by name
  then id); commands `AddLeader`, `RemoveLeader`, `MoveLeaderUp`, `MoveLeaderDown`, each greyed when it can't act.
- `docs/map.md`; the backlog's chosen-row line goes.

## Tests
- `NewGame/NewGameLeaderListTests` (new; `NewGameFixtures` gains its own five members, Vela, Ilsa and Brom leading,
  Cato and Zora not, Zora's id `abel` sorting first): the leaders in their order, the first chosen, a missing one shown
  and named by Data; the record's list kept untouched; moved up and down; `LeadersToAdd` by name; Add after the chosen
  row, last with none, choosing it; Remove choosing the next and the last one's Data line; each command greyed at its
  bound and re-raised by a move; the list's notices.
- `Events/ListOrderTests`: `CanMoveUp` and `CanMoveDown` at each row and with none chosen.
- `Catalogs/ListItemWatchTests` (new): the rows held, added, removed and moved.

## Verify by hand
None: the page comes in NG3.
- **Done, with deviations:** split at its start: the starting items are NG2c (the extraction made this part pass 320
  lines again). Decision 1019: the shared piece is the move greying (`ListOrder.CanMoveUp`, `CanMoveDown`) and the row
  watching (`ListItemWatch.WatchRowsOf`), not a list type: each list's change step raises its own notices and its view
  model keeps the commands its page binds; the Classes tests ran unchanged. The new game raises `Problem`'s change for
  its list, no `Changed` event (no list above it relays one). `NewGameFixtures` now holds its own five members (the
  scholar fixture renamed), so NG2's tests no longer lean on the repository's. Beyond the list: `SelectedLeader`
  re-raising Remove, Up and Down. Sizes: `NewGameViewModel` 122, `.Leaders` 95, `ListOrder` 58, `ListItemWatch` 48,
  `ClassViewModel.Skills` 119, `.Resources` 125; tests `NewGameLeaderListTests` 223, `ListOrderTests` 70,
  `ListItemWatchTests` 62, `NewGameFixtures` 45 (about 70 lines of code net, 300 of tests; 36 tests).
