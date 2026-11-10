# MF4. New folder and Move to folder: the rules

Editor, about 200 lines. Builds on MF3 and design.md, "Key decisions" 8. Decisions 533, 534 (Q13, Q14). The window, the context menus and the drag come in MF5.

## Goal
The editor can make an empty folder (kept for the session) and move a map into a folder: a move shows the moved map and is one settings step on it, so Ctrl+Z undoes it, Ctrl+S saves it, and the row moves at once with " *".

## New types
- `Maps/MapFolderChoiceViewModel`: the folder dialog's path, typed or picked, and its problem.
  - Constructor `(IReadOnlyList<string> knownFolders, string? startPath)`; `Folders` (every folder of the saved and open maps and the session's, sorted, with "(top of the list)" first); `[ObservableProperty] PathText`; `Problem` (`MapFolderPath.ProblemOf`; empty means the top); `ToPath()` (null for the top), throwing on a problem.
- `Shell/MainViewModel.MapFolders.cs` (partial): new folders, moving a map and (MF6) renaming a folder.
  - `public MapFolderChoiceViewModel StartNewFolder(string? parentPath)` (the path offered starts with the parent and the separator); `public bool TryAddFolder(MapFolderChoiceViewModel choice)`: `Navigation.AddSessionFolder`.
  - `public MapFolderChoiceViewModel StartMovingMap(string mapId)` (starting on the map's folder); `public bool TryMoveMap(string mapId, MapFolderChoiceViewModel choice)` and `public bool TryMoveMapTo(string mapId, string? folderPath)` (a drag: no dialog): `TryOpenMap(mapId)` first (shown, so Ctrl+Z reaches the step, decision 503), then `CurrentMap.Do(new SetMapSettingsEdit(settings, settings with { Folder = path }))`; nothing when the folder is the same.

## Tests
`Editor.Tests/Maps/MapFolderChoiceViewModelTests` (new):
- `Folders_SavedOpenAndSession_ListedOnceSortedWithTheTopFirst`.
- `Problem_BadPath_NamesTheRule`; `Problem_Empty_IsNull` (the top).
- `ToPath_Top_IsNull`; `ToPath_Typed_GivesIt`.
`Editor.Tests/Shell/MainViewModelMapFoldersTests` (new, a copy of the test world):
- `TryMoveMap_HiddenMap_ShowsItAndMovesIt` (start moved while forest is shown: start shown, its settings' folder set, its row marked).
- `TryMoveMap_Moved_Undo_PutsItBack`.
- `TryMoveMap_SameFolder_RecordsNothing`.
- `TryMoveMap_Saved_WritesTheFolder` (Ctrl+S, the file holds it).
- `TryMoveMapTo_Top_ClearsTheFolder`.
- `TryAddFolder_Empty_ShownInTheTree`; `TryAddFolder_BadPath_AddsNothing`.
- `StartNewFolder_UnderAFolder_OffersItsPath`.

## Verify by hand
Rules only; MF5 adds the menus, the dialog and the drag.
