# RN2. The rename's rules: `MapRenamingViewModel` and `MainViewModel.MapRenaming.cs`

Editor, about 230 lines. Builds on RN1 and design.md, "Key decisions" 7. Decisions 530, 531 (Q10).

## Goal
The editor renames the shown map's id: the new id is typed, prefilled from the display name; its problems are named; the rename can't start while anything is unsaved; it is written at once through `ContentWriter.RenameMap`, and the editor carries on from the written content with the map shown under its new id, Back to previous map (Alt+Left) still working, and every map's undo history starting afresh.

## New types
- `Maps/MapRenamingViewModel`: the Rename map dialog's new id, prefilled, and its problem (`StartX`/`TryApplyX` pattern, as the other dialogs).
  - Constructor `(MapDefinition map, IReadOnlyCollection<string> takenIds)`; `OldId`; `[ObservableProperty] NewId`, prefilled with `MapIds.FromName(map.Settings.DisplayName, takenIds without the old id)`, so a map named "Primeval Growth Crossroad" proposes `primeval_growth_crossroad`.
  - `Problem`: empty; not lower-case letters, digits and underscores (the rule `MapIds` makes; a small `MapIds.IsWellFormed(string id)` added beside `FromName` with its test); unchanged ("The id is already this map's."); taken by a saved or an open map ("Map 'forest' already has this id.", compared without case, as `MapIds` does, since ids are file names).
- `Shell/MainViewModel.MapRenaming.cs` (partial): renaming the shown map's id: starting it, refusing it, writing it.
  - `public bool CanRenameMap => CurrentMap is not null && !HasUnsavedChanges;` raised whenever either changes (RN3 greys the entries with it); `public string RenameMapTip` ("Change the shown map's id; every warp into it and the new game's start follow." or, while unsaved, "Save all first (Ctrl+Shift+S): a rename writes every map that leads to this one.").
  - `public MapRenamingViewModel? StartRenamingMap()`: null while `CanRenameMap` is false; the taken ids are the saved maps' and the open maps'.
  - `public bool TryRenameMap(MapRenamingViewModel renaming)`:
    - with `Problem` set, false;
    - while `HasUnsavedChanges` (the modal dialog rules a change out, but a refusal is the backstop): false, `SaveError` = the tip's "Save all first" sentence;
    - else `_writer.RenameMap(Content.Current, old, new)` in `TryWrite`; on success, in this order:
      1. `Navigation.RenameMap(oldId, newId)`: the shown and previous ids it keeps for Back are renamed (a new `MapNavigation` method), so Back still reaches the renamed map, or returns from it to the map shown before;
      2. `_shownAfterRewrite = newId` (a field `ShowRewritten` reads: the map to show after the rewrite, null meaning the shown map's own id, as for the purge);
      3. `Content.ReplaceAfterRewrite(renamed)`: the journal clears, the clean open maps are rebuilt, and `ShowRewritten` shows the renamed map straight away (opening it from the written content, since `OpenMaps.ReplaceFrom` drops the old id), never passing through "no map shown", which would make `Navigation.NoteShown(null)` lose the previous map.
  - Its summary names decision 442's precedent.
- `Maps/Settings/MapSettingsViewModel`'s summary and `Maps/Editing/MapIds`' summary lose "the id can't change (question 70)": the id changes through Map > Rename map... (grep "question 70" in `src/` and `docs/map.md`).

## Tests
`Editor.Tests/Maps/MapRenamingViewModelTests` (new):
- `NewId_StartsFromTheDisplayName` (a map "Old Mill" with id `map_3` proposes `old_mill`).
- `NewId_DisplayNameMatchesAnotherMap_ProposesTheNextFree` (`old_mill_2`).
- `Problem_SameAsOld_SaysSo`; `Problem_Taken_NamesTheMap`; `Problem_TakenWithOtherCase_NamesTheMap`; `Problem_Uppercase_NamesTheRule`; `Problem_Empty_NamesTheRule`; `Problem_WellFormedAndFree_IsNull`.
`Editor.Tests/Maps/Editing/MapIdsTests`: `IsWellFormed_SnakeCase_IsTrue`, `IsWellFormed_SpaceOrUpper_IsFalse`.
`Editor.Tests/Shell/MapNavigationTests` (154 lines): `RenameMap_PreviousMapRenamed_GoBackOpensTheNewId`.
`Editor.Tests/Shell/MainViewModelMapRenamingTests` (new, a copy of the test world):
- `TryRenameMap_Saved_WritesAndShowsTheMapUnderItsNewId` (`CurrentMap.Map.Id`, the map list's row, the files on disk).
- `TryRenameMap_AnotherMapUnsaved_RefusesAndWritesNothing` (the "Save all first" line; the files unchanged).
- `CanRenameMap_AnythingUnsaved_IsFalse` and `CanRenameMap_PropertyChangedWhenAMapIsEdited_IsRaised`.
- `TryRenameMap_Renamed_NothingToUndo` (`History.UndoCommand.CanExecute` false).
- `TryRenameMap_Renamed_WarpStepsOfOpenMapsFollow` (another map opened before the rename shows the new id in its warp step).
- `TryRenameMap_ThenGoBack_ReturnsToTheMapShownBefore` (start shown, then forest; forest renamed; Back shows start).
- `TryRenameMap_ThenGoBackTwice_ReturnsToTheRenamedMap` (start shown, then forest; forest renamed; Back shows start; Back again shows the forest under its new id).
- `StartRenamingMap_NoMapShown_IsNull`.

## Verify by hand
Tests only; RN3 adds the window.
