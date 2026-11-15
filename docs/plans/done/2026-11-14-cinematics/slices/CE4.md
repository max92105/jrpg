# CE4. One cinematic as edited

Editor, about 200 lines (about 110 of them tests). Needs CE3, CE1 (`OfCinematics`). Design: "Structure" (Editor).

## Goal
A cinematic's id is typed while new and fixed once the draft holds it; its track is chosen or none; its stills are
added after the chosen one from a picture, removed and moved, renumbered after each change; its problem is the typed
id, then the first row's problem.

## New types
- `Editor/Cinematics/CinematicViewModel` (internal sealed partial, `ObservableObject`): one cinematic as edited: its id
  (typed while new), its track and its still rows in order.
  - `string Id` (typed while `IsNew`), `bool IsNew`, `string MusicFile` ("" for none, the box's first entry).
  - `ObservableCollection<CinematicStillRow> Stills`, `CinematicStillRow? SelectedStill`.
  - `AddStill(string imageFile)`: a `CinematicStillRow.New` after the chosen one (at the end when none), chosen.
  - `RemoveStill()`, `MoveStillUp()`, `MoveStillDown()` through `Events/ListOrder` (as the drop table's entries);
    `Number`s renumbered after each.
  - `string? Problem`: the id while new (`Objects/ObjectIds`' rule, "Cinematic id: ..."), "Add at least one still.",
    then the first row's problem. The list's problem (Data's) is CE5's.
  - `event EventHandler? StillsChanged` (typed fields and list changes only, relayed from each row's `Changed`).
  - `static CinematicViewModel From(CinematicDefinition)`, `static CinematicViewModel New(string id)`;
    `CinematicDefinition ToCinematic()` (throws while a problem shows).

## Tests
`CinematicViewModelTests` (new), rows from records built in the test:
- `From_Cinematic_ListsStillsInOrderNumberedFromOne`.
- `AddStill_StillChosen_InsertsAfterItAndChoosesIt`; `AddStill_NoneChosen_AppendsIt`.
- `RemoveStill_Middle_RenumbersTheRest`.
- `MoveStillDown_First_SwapsAndRenumbers`; `MoveStillUp_First_DoesNothing` (the guard).
- `Problem_NewWithBadId_NamesTheId`; `Problem_NoStills_AsksForOne`; `Problem_RowProblem_IsTheFirstRows`.
- `ToCinematic_NoTrack_HasNullMusic`; `ToCinematic_Track_KeepsIt`.
- `StillsChanged_TypingInARow_IsRaised`; `StillsChanged_Moving_IsRaised`.

## Verify by hand
None: no window yet.

- **Done, with deviations:** Add takes the picture from an `ImageToAdd` box through `AddStillCommand`, as the drop table's `ItemToAdd` does, rather than an `AddStill(string)` method. `Problem` is raised on every row change and list change (the window's line reads it). Tests added: `AddStillCommand_NoPicturePicked_CannotExecute`, `Problem_RowTyped_IsRaised`, `StillsChanged_TypingInAMovedRow_IsRaisedOnce`.
  Also: `New()` takes no id (the list sets it after, as the drop table's does), and a new cinematic's id problem is the bare `ObjectIds` message, which the list prefixes with "Cinematic #n:" (`RecordListProblem`). CE4's review: the row watch is shared as `Catalogs/ListItemWatch` (the third copy; the quest and drop table copies are a backlog line).
