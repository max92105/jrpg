# WD2. `CatalogDialog` and the Shops page

**Stage 1.** Needs WD1.

**Goal:** one modal dialog that hosts a Database page at the main window's size: its title, the page, the problem
line, OK greyed by it (the default button) and Cancel (the cancel button). The Shops window becomes the first page in
it. The six other windows open at the main window's size too, until each converts. The developer gets the screen
space with no change to the undo rule. Design: [design.md](../design.md), section 3 (stages 1 and 2); decisions 930,
937; the recipe: [page-recipe.md](../page-recipe.md).

## New types (in `Workspaces/`)
- `IEditedCatalog`: a page's edited catalog as its hosts see it: its problem and its chosen record's key (three
  members, design section 3).
- `CatalogDialog` (window): hosts one page modally: title, page, problem line (`ProblemTextBrush`), OK
  (`IsDefault`, greyed while the page's `Problem` is set) and Cancel (`IsCancel`); constructor `(string title,
  FrameworkElement page, IEditedCatalog edited)`, `EditorWindow` style.
- `DialogBounds`: places a Database dialog over its owner at the owner's size: `static void FitTo(Window dialog, Window
  owner)` (the owner's `WindowState` when maximized, else its left, top, width and height).

## New page
- `Trade/ShopCatalogPage` (user control): the Shops window's layout by the recipe (no OK, Cancel, problem line; typed
  boxes `PropertyChanged`; `AncestorType=trade:ShopCatalogPage`).

## Changes
- `Trade/ShopCatalogViewModel`: implements `IEditedCatalog` (`ChosenKey` = `SelectedShop?.Id`, `Choose`).
- `Shell/MainWindow.Database.cs`: `OpenInDialog<TEdited>(title, start, apply, buildPage)` (design section 3); the
  Shops opener uses it; the other five openers (and `MainWindow.FieldSprites`) call `DialogBounds.FitTo` on their
  window before `ShowDialog`.
- `Shell/MainViewModel.Database.cs`: `HasShopEditing` goes with its menu binding in `DatabaseMenu.xaml`.
- Deleted: `Trade/ShopCatalogWindow.xaml`, `.xaml.cs`.
- `docs/map.md`: a `Workspaces` row (`CatalogDialog`, `DialogBounds`, `IEditedCatalog`); the Trade row names the page.

## Tests
`tests/Jrpg.Editor.Tests/Trade/ShopCatalogViewModelTests.cs` (202 → about 230), shops built in the test:
- `ShopCatalogViewModelTests: Choose_KnownId_ChoosesIt`
- `ShopCatalogViewModelTests: Choose_UnknownId_KeepsTheChoice`
- `ShopCatalogViewModelTests: ChosenKey_NewShopTyped_IsItsTypedId`

## Size
About 150 lines of code and XAML (50 moved), 30 of tests, 50 of window sizing lines: about 230.

## Verify by hand
Agents may run it (no art in Shops). `open-editor-windows.ps1`: "Database>Shops...: opened" with a screenshot the size
of the main window, the other five the same size, no `ERROR`, no binding-error line. In the editor: Database >
Shops...: rename a shop, Enter: applied (Ctrl+Z puts it back); add a shop with an empty id: the problem line shows,
OK greys, Esc drops it.
- **Done, with deviations:** `ShopCatalogViewModel.CanApply` and its two tests removed (the dialog greys OK on
  `Problem`); new files 159 lines; tests 202 → 210 (`wc -l`); `ChosenKey` raises no change (its first reader, WS2,
  reads it on demand).
