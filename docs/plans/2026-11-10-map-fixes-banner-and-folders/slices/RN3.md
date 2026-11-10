# RN3. The Rename map window

Editor, about 125 lines. Builds on RN2 and ML0 (`Shell/MapListPanel`). Decision 530.

## Goal
Map > Rename map... and the map list's right-click **Rename map...** open a small dark dialog on the shown map: the old id, the new id prefilled, its problem, and OK greyed while there is one; OK renames at once. Both entries are greyed while anything is unsaved, their tooltip saying why.

## New types
- `Maps/MapRenameWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the Rename map dialog. Rows: "Map: {display name}", "Old id: forest", "New id" box (selected on opening), the problem line, a note "Every warp into this map and the new game's start follow. Written at once: Ctrl+Z can't undo it, and every map's undo history clears; rename it back instead.", OK and Cancel. `public static bool TryRename(Window owner, MapRenamingViewModel renaming, Func<MapRenamingViewModel, bool> tryRename)`; OK calls `tryRename` and closes on true.

## Changes
- `Shell/MainWindow.xaml` (about 287 after ML0): in the Map menu, `<MenuItem Header="Rename map..." Click="OnRenameMapClicked" IsEnabled="{Binding CanRenameMap}" ToolTip="{Binding RenameMapTip}" ToolTipService.ShowOnDisabled="True" />` (+1, ends about 288).
- `Shell/MapListPanel.xaml`: a context menu on the list's rows with **Rename map...**, bound the same way (`CanRenameMap`, `RenameMapTip`, shown on disabled): right-click selects the row (opening the map, as a click does) then raises a `RenameRequested` event the window handles with the same method. MF3 carries this entry over to the tree's map rows.
- `Shell/MainWindow.MapSwitching.cs`: `OnRenameMapClicked` (and the panel's event) opens `MapRenameWindow` with `_viewModel.StartRenamingMap()` and `_viewModel.TryRenameMap`.
- `Shell/ControlsHelpWindow.xaml`: the line "Map > Rename map... (or right click a map in the list): a new id for the shown map; greyed while anything is unsaved."

## Tests
Window code only: RN2's tests cover the rules (`CanRenameMap`, `RenameMapTip` and their change events). None new.

## Verify by hand
1. Open "to_aslas_1" (display name "Primeval Growth Crossroad"); Map > Rename map...: the new id box shows `primeval_growth_crossroad`, selected; the note says every map's undo history clears.
2. Type `forest`: the problem line names map 'forest' and OK is greyed. Type `primeval_crossroad`: OK.
3. The map list's row reads "Primeval Growth Crossroad (primeval_crossroad)"; `content/maps/` and `content/map-layouts/` hold `primeval_crossroad.json` and no `to_aslas_1.json`; `git diff` shows every map whose warp led there changed only on those lines.
4. Alt+Left returns to the map shown before the rename; Alt+Left again comes back to the renamed map.
5. Open a map that warps into it; Events tab; click the door: its warp step says "Warp to: primeval_crossroad (...)"; Go to arrives there.
6. Paint a cell on any map: Map > Rename map... is greyed, hovering it says "Save all first..."; the list's right-click entry too. Ctrl+Z the paint: both are enabled again.
7. F5 the game, load a save made on the old map: it loads, the party at the new game's start (the save repaired, decision 531); walk to the renamed map: it loads.
8. Right-click a map row in the list: Rename map... opens on that map.
9. The repository tests that pin the old id fail after a real rename (`dotnet test`): re-pin them in the same commit as the content rename (design.md, "Risks"). For this check, revert with `git checkout -- content` and delete the new files instead.
