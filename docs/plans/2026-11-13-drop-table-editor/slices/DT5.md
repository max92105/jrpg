# DT5. The Drop tables window and Database > Drop tables...

Editor, about 170 lines (window XAML and code, menu wiring). Design: key decision 1, "Risks" (file sizes, the row selection). Decisions 596, 598, 603, 605, 606. Needs DT4.

## Goal
Database > **Drop tables...** opens the Drop tables window: tables on the left with Add table and Remove; the chosen table on the right: its id and the "Rolled by" line under it, then its entries as rows (`[i]`, item, Chance %, Min, Max) under column titles, with the item box, Add entry, Remove, Up and Down under them, and the fixed note "Each entry rolls on its own; the chances don't add up." (decision 606). The problem line and OK greyed through `CanApply`; Cancel drops the session.

## New types
- `Editor/Loot/DropTableCatalogWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the window above, laid out as `ShopCatalogWindow`, its right panel greyed through the theme's `EnabledWhenChosen` (DT0).
  - The entries list: a `ListBox` (`SelectedItem` bound to `SelectedEntry`) whose item template holds the `[i]` label, the item label and three `TextBox`es (`UpdateSourceTrigger=PropertyChanged`; a local number-box style `BasedOn="{StaticResource {x:Type TextBox}}"`, 50 wide: the second copy of the Enemies window's `NumberBox`, so it stays local).
  - Its `ItemContainerStyle` (`BasedOn="{StaticResource {x:Type ListBoxItem}}"`) holds `<EventSetter Event="PreviewGotKeyboardFocus" Handler="OnEntryRowFocused"/>`, so a click into a row's box chooses that row and Remove, Up and Down act on it. Not an `IsKeyboardFocusWithin` trigger with an `IsSelected` setter: the trigger lets go of the selection as soon as a button takes the focus, and `ObservableCollection.Move` rebuilds the row's container (the devil's advocate's blocker). The selection then lives in the view model's `SelectedEntry`, which Up and Down keep (DT3).
  - The `RolledBy` line under Id, muted, tooltip "The saved and applied enemies; an open Enemies window's unapplied choices aren't counted".
  - Code-behind, view-only: the constructor (`DataContext`), `OnOkClicked`, and `OnEntryRowFocused`, one line: `((ListBoxItem)sender).IsSelected = true;`.
  - Tooltips: Id "Lowercase letters, digits and _; fixed once saved, since enemies name it"; Remove (table) "Only a table added this session"; Chance "Whole percent, 1 to 100, before drop luck".

## Changes
- `Editor/Shell/MainViewModel.Database.cs`: `public DropTableEditingViewModel? DropTableEditing { get; }` and `HasDropTableEditing`, beside `ShopEditing` and `EnemyEditing`; the partial's summary names the drop tables.
- `Editor/Shell/MainViewModel.cs` (281 → 282): the constructor sets `DropTableEditing = new DropTableEditingViewModel(Content);` beside `ShopEditing` (decision 605).
- `Editor/Shell/MainWindow.Database.cs`: `OnEditDropTablesClicked` (start, show with `Owner = this`, apply on OK), as `OnEditShopsClicked`; the summary names the drop tables.
- `Editor/Shell/MainWindow.xaml` (290 → 291): `Drop tables...` between `Shops...` and `Enemies...`, `IsEnabled="{Binding HasDropTableEditing}"`, tooltip "Each table's items, chances and quantities; the Enemies window's Drop table box offers them."
- `docs/plans/backlog.md`: the `MainViewModel.cs` size line (updated at approval) gets the real count after this slice.
- `docs/map.md`: the `Loot` row names the window and the menu entry.

## Tests
None new: window code; DT2-DT4 test the rules.

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`. Menu **Database > Drop tables...**: the window lists `goblin`; chosen, "Rolled by: goblin, goblin_chief, stone_beetle" (on 2026-11-13) under its id, eight rows from `[0] Potion (potion)` 30 % to `[7] Essence (essence)` 5 %, `[6] Scrap (scrap)` with Min 1, Max 2, and the note "Each entry rolls on its own; the chances don't add up."
2. Click **Add table**: "(new table)" appears, chosen, the "Rolled by" line empty, the problem line names the empty id and OK is greyed. Type `slime` in **Id**: the problem line says the table must list at least one item (Data's words).
3. In the item box under the entries pick **Potion (potion)**, click **Add entry**: row `[0] Potion (potion)`, Chance 10, Min 1, Max 1; OK lights up. Type `abc` in its Chance: the line reads "Drop table slime: entries[0] (potion): Chance must be a whole number (was 'abc')."; type `0`: Data's range message; type `25`.
4. Pick **Scrap (scrap)**, **Add entry**; Max `3`. Click into the Potion row's Chance box and click **Down**: Potion moves below Scrap and the indexes renumber.
5. Add **Essence (essence)** as a third row. Click into the Essence row's **Min** box, type `1` over it, then click **Up** twice: Essence moves to `[1]`, then to `[0]`, staying the chosen row after each click (the second click acts on it, not on whatever row the first move left highlighted). Then click **Remove**: Essence goes, not another row.
6. Click **Remove** under the tables with `goblin` chosen: greyed. Click **OK**.
7. **Database > Enemies...**: choose `goblin`; its **Drop table** box lists `slime`. Pick it, **OK**.
8. **Ctrl+Z**: goblin's table is `goblin` again (reopen Enemies to see). **Ctrl+Z** again: Drop tables no longer lists `slime`. **Ctrl+Y** twice.
9. **Ctrl+S**: `git diff content/` shows `drop-tables.json` with the `slime` table (Scrap with `"maxQuantity": 3`, Potion with only `"chance": 25`) and `enemies.json` with goblin's `dropTableId` changed; no other file.
10. To see it in play, give `slime`'s Potion a chance of `100` before saving, start the game (`dotnet run --project src/Jrpg.Game`), win a battle against a goblin: the victory's Items step lists the potion.
11. Revert with `git checkout -- content/drop-tables.json content/enemies.json` if this was only a check.
