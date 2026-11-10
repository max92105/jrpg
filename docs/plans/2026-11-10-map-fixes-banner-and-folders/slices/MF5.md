# MF5. The folder dialog, the map list's context menus and the drag

Editor, about 160 lines. Builds on MF4 and MF3 (the map rows' context menu holding Rename map...). Decisions 532, 533, 534.

## Goal
The developer organises the list from the list: right-click **New folder...** (under the clicked folder, or at the top on empty space), right-click a map **Move to folder...** (pick a folder or type a new path; "(top of the list)" for none), or drag a map row onto a folder row (or onto the empty space below the rows for the top).

## New types
- `Maps/MapFolderWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the dialog for New folder, Move to folder and (MF6) Rename folder: a title and an OK label given by the caller, an editable combo box over `Folders` bound to `PathText` (the dark combo box template), the problem line, OK greyed while it shows. `public static bool TryChoose(Window owner, string title, MapFolderChoiceViewModel choice, Func<MapFolderChoiceViewModel, bool> apply)`.

## Changes
- `Shell/MapListPanel.xaml` / `.xaml.cs`: the context menus (folder rows: New folder..., Rename folder... greyed until MF6; map rows: Move to folder... and New folder... added beside MF3's Rename map...; empty space: New folder...), each raised as an event the window handles; drag and drop of map rows onto folder rows and the empty area (`AllowDrop`; the drop calls `TryMoveMapTo`); a drag starts only past `SystemParameters.MinimumHorizontalDragDistance`, so a click still opens a map.
- `Shell/MainWindow.MapSwitching.cs` (about 70 after ML0): the handlers opening `MapFolderWindow` with MF4's start and apply methods.
- `Shell/ControlsHelpWindow.xaml`: "Map list: right click for New folder and Move to folder; drag a map onto a folder, or below the rows for the top."

## Tests
Window code only; MF4's tests cover the rules. None new.

## Verify by hand
1. Right-click empty space in the map list: New folder...: type `Act 1`, OK: an empty "Act 1" folder shows.
2. Right-click "Act 1": New folder...: the box offers `Act 1/`; type `Act 1/Village`, OK: "Village" under "Act 1".
3. Right-click the start map's row: Move to folder...: pick `Act 1/Village`, OK: the start map shows, its row under "Village" with " *". Ctrl+Z: back at the top; Ctrl+Y.
4. Drag the forest's row onto "Act 1": it shows and moves there, marked. A plain click on a row still opens its map without moving it. Drag it below the rows: back at the top.
5. File > Save all: the marks go; `git diff content/maps` shows only `folder` lines.
6. Close and reopen the editor: the folders holding maps are there; an empty folder from step 1 without maps is gone (decision 534).
7. Revert with `git checkout -- content` if this was only a check.
