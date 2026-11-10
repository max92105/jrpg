# MF3. The map list panel shows the tree

Editor, about 170 lines. Builds on MF2, ML0 (`Shell/MapListPanel`) and RN3 (the list's Rename map... entry).

## Goal
The map list is a tree: folders open and close with their arrow (or Left and Right), a click on a map row opens the map, the shown map's row stays selected (its folders opened to show it) after a save, an edit's mark or a filter change; the filter box works as before over the tree; a map row's right click still offers **Rename map...** (RN3's entry, carried over here, not left to MF5).

## Changes
- `Shell/MapListPanel.xaml`: the `ListBox` becomes a `TreeView` over `Navigation.ShownTree` with a `HierarchicalDataTemplate` on `Rows` (folders with a folder glyph, maps with their text and " *"); `TreeViewItem`'s `IsExpanded` and `IsSelected` bound two way to the node.
- `Shell/MapListPanel.xaml.cs`: a selected map node opens it (`TryOpenMap`); a selected folder node opens nothing; `SelectMap(mapId)` (ML0's) finds the node, opens its folders and selects it; expanding or collapsing calls `Navigation.NoteExpanded`.
- RN3's context menu moves onto the map rows' item template (a folder row has none until MF5): **Rename map...** with its `CanRenameMap` greying and `RenameMapTip`, right click selecting the row first, the same `RenameRequested` event.
- The dark theme: `TreeView` and `TreeViewItem` need templates (WPF's defaults are light); `Shell/EditorTemplates.xaml` is at 290 lines, so they go in a new `Shell/EditorTreeTemplates.xaml` merged where `EditorTemplates.xaml` is.
- `Shell/ControlsHelpWindow.xaml`: the map list line names folders (Left and Right close and open them).

## Tests
The selection and opening are window code. The rule they read is a new `MapTree.PathTo(IReadOnlyList<MapTreeNode> rows, string mapId)` (the folder nodes above a map, top first), tested in `MapTreeTests` (MF2's file):
- `PathTo_NestedMap_GivesEveryFolderAbove`.
- `PathTo_TopLevelMap_IsEmpty`.
- `PathTo_UnknownMap_IsEmpty`.

## Verify by hand
1. With `forest` in "Act 1/Woods" (MF1's check): the list shows "Act 1" with "Woods" under it, holding "Forest (forest)"; the other maps below the folders.
2. Click "Act 1"'s arrow: it closes; Left and Right on a selected folder close and open it.
3. Click the forest row: the forest opens. Open another map, then Alt+Left: the forest row is selected again, its folders opened.
4. Paint a cell: the row shows " *" and stays selected; Ctrl+S: the mark goes, the folders stay as they were.
5. Ctrl+M, type `for`: only "Act 1 / Woods / Forest" shows, opened; Enter opens it; Esc: the whole tree is back with the folders as before.
6. The dark theme: no white row, arrow or scroll bar.
7. Right click the forest row: Rename map... opens the rename dialog on the forest (greyed while anything is unsaved); right click a folder: no menu yet.
