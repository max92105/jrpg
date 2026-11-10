# MF6. Rename folder

Editor, about 170 lines. Builds on MF5, SV1 (`ContentWriter.WriteMaps`) and RN2's "written at once" path (design.md, "Key decisions" 7). Decision 533 (Q13).

## Goal
Right-click a folder, **Rename folder...**: every map in it and in its subfolders gets the new path, written at once; refused while anything is unsaved; the undo history starts afresh, as after a map id rename. Renaming an empty session folder only renames it in the list. A folder moves under another one by typing its new path ("Act 1/Woods" to "Act 2/Woods").

## Changes
- `Shell/MainViewModel.MapFolders.cs` (MF4's):
  - `public MapFolderChoiceViewModel StartRenamingFolder(string folderPath)`;
  - `public bool TryRenameFolder(string folderPath, MapFolderChoiceViewModel choice)`:
    - a problem, the top (empty), or the same path: false;
    - while `HasUnsavedChanges`: false, `SaveError` = "Save all first (Ctrl+Shift+S): renaming a folder writes every map in it.";
    - the content with every map whose folder `MapFolderPath.IsWithin(folder, folderPath)` re-rooted under the new path (one private `Rerooted(string path, string oldFolder, string newFolder)`, tested through the command); with no map in it, only the session folders change;
    - `_writer.WriteMaps(content, changedIds)` in `TryWrite`, then `Content.ReplaceAfterRewrite(content)`, the session and expanded folders renamed (`MapNavigation.RenameFolders(oldPath, newPath)`), the shown map shown again.
- `Shell/MapListPanel.xaml`: Rename folder... enabled; `Shell/MainWindow.MapSwitching.cs`: its handler opens `MapFolderWindow` with the start and apply methods.
- `Shell/ControlsHelpWindow.xaml`: "Rename folder: written at once, every map in it; refused while anything is unsaved."

## Tests
`Editor.Tests/Shell/MainViewModelMapFoldersTests` (MF4's file, about 130 lines then; this adds about 80):
- `TryRenameFolder_TwoMapsInIt_WritesBoth` (their files hold the new path).
- `TryRenameFolder_Subfolder_FollowsTheNewRoot` ("Act 1/Village" becomes "Act 2/Village").
- `TryRenameFolder_SimilarName_LeftAlone` ("Act 10" untouched when "Act 1" is renamed).
- `TryRenameFolder_AnythingUnsaved_RefusesAndWritesNothing`.
- `TryRenameFolder_EmptySessionFolder_RenamesItOnly` (no file written).
- `TryRenameFolder_Renamed_NothingToUndo`.
`Editor.Tests/Shell/MapNavigationTreeTests`: `RenameFolders_ExpandedSubfolder_StaysExpandedUnderTheNewPath`.

## Verify by hand
1. With "Act 1/Village" holding the start map and "Act 1" holding the forest (MF5's check, saved): right-click "Act 1", Rename folder...: `Chapter 1`, OK.
2. The tree shows "Chapter 1" with "Village" under it; both maps' files hold the new paths; nothing is marked unsaved; Edit > Undo is greyed.
3. Paint a cell, then Rename folder...: refused, the error line says "Save all first". Ctrl+Z the paint.
4. Rename "Chapter 1/Village" to `Chapter 2/Village`: "Chapter 2" appears holding "Village"; "Chapter 1" keeps the forest.
5. Revert with `git checkout -- content` if this was only a check.

- **Done, with deviations:** the re-rooting rule is Core's `MapFolderPath.Moved(path, oldFolder, newFolder)` (tested there), not a private of the view model, so the navigation's folders follow with the same rule (`MapNavigation.RenameFolders` over the session's and the open folders). The choice starts from `MapFolderChoiceViewModel.ForRenamingFolder`: the unchanged path and the top are problems ("The folder already has this path.", "Type the folder's new path."), so OK greys instead of a refused OK. `CanRenameFolder` and `RenameFolderTip` grey the entry with the reason, raised with `CanRenameMap`. The tests are in their own file, `MainViewModelFolderRenamingTests` (the folders file was near its limit); the subfolder case is covered by the two-maps test (start in "Act 1/Village").
  Review fix: `MapNavigation.Follow` removes every folder within the old path, then adds them all moved, so a folder renamed into its own subfolder ("A" to "A/A") moves each folder once, whatever order the set gives them (tested).
