# WD4. The Drop tables page

**Stage 2.** Recipe: [page-recipe.md](../page-recipe.md), "A window to convert" (step 0 first).

**Goal:** the Drop tables window becomes a page in `CatalogDialog`, from the menu and from the Enemies window's
Edit... (a dialog over the Enemies window, on the enemy's table; its OK applies its own step at once and the Enemies
window takes the new tables, decision 602 kept until WS6).

## New page
- `Loot/DropTableCatalogPage` (user control): the window's layout without OK and Cancel; its row-choosing
  `PreviewGotKeyboardFocus` setter kept.

## Changes
- `Loot/DropTableCatalogViewModel`: implements `IEditedCatalog`: `ChooseTable` renamed `Choose`, `ChosenKey` =
  `SelectedTable?.Id`.
- `Shell/MainWindow.Database.cs`: `EditDropTables(Window owner, string chosenTableId)` builds a `CatalogDialog` with
  the page, owned by the given window and sized to the main window (`DialogBounds.FitTo(dialog, this)`), `Choose` on
  the table, apply on OK, returning whether it applied, as before; the menu's opener calls it with the main window.
- `HasDropTableEditing` and its binding go.
- Deleted: `Loot/DropTableCatalogWindow.xaml`, `.xaml.cs`. `docs/map.md`: the Loot row.

## Tests
`tests/Jrpg.Editor.Tests/Loot/DropTableCatalogViewModelTests.cs` (254): `ChooseTable_KnownId_ChoosesIt` and
`ChooseTable_EmptyId_KeepsTheFirst` renamed `Choose_KnownId_ChoosesIt` and `Choose_EmptyId_KeepsTheFirst` in place;
one more, `ChosenKey_ChosenTable_IsItsId`, goes in a new sibling `DropTableCatalogViewModelChoiceTests` (the base file
stays at 254).

## Verify by hand
Agents may run it. The script: "Database>Drop tables...: opened". In the editor: Enemies, an enemy with a table,
Edit...: the Drop tables dialog on that table over the Enemies window; add a table, OK: the Enemies window's box offers
it; Cancel the Enemies window: the table stays (its own step), Ctrl+Z takes it back.
- **Done, with deviations:** `CanApply` removed (the window was its only reader): its two tests renamed
  `ToTables_ProblemShown_Throws` and `ToTables_NewTableWithOneEntry_ListsIt`, a third's `CanApply` assert dropped, so
  the base test file is 252 lines; the Battle folder's, its test's and the map's Battle row's "window" say "page".
