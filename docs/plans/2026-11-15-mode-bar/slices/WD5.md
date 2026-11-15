# WD5. The Enemies page

**Stage 2.** Recipe: [page-recipe.md](../page-recipe.md), "A window to convert" (step 0 first: the window is 205 / 205
lines on 2026-11-15; row 8 later adds its Tactics tab to the page).

**Goal:** the Enemies window becomes a page in `CatalogDialog`; its Edit... beside the Drop table box opens the Drop
tables dialog over the Enemies dialog (WD4), the nested refresh kept (decision 602, until WS6).

## New page
- `Battle/EnemyCatalogPage` (user control): the window's layout and code-behind without OK and Cancel; constructor
  `(EnemyCatalogViewModel enemies, EnemyArtSources art, Action<Window, string> editDropTables)`, the window passed
  being `Window.GetWindow(this)` (the dialog).

## Changes
- `Battle/EnemyCatalogViewModel`: implements `IEditedCatalog` (`ChosenKey` = `SelectedEnemy?.Id`, `Choose`).
- The page's code-behind: `RenderTick` started on `Loaded` and stopped on `Unloaded` (was the constructor and
  `Closed`); its file dialogs and the GIF options' owner are `Window.GetWindow(this)`.
- `Shell/MainWindow.Database.cs`: the Enemies opener through `OpenInDialog`, its page built with the art sources and
  the `EditDropTables` callback followed by `RefreshDropTables`, as today.
- `HasEnemyEditing` and its binding go.
- Deleted: `Battle/EnemyCatalogWindow.xaml`, `.xaml.cs`. `docs/map.md`: the Battle row.

## Tests
`tests/Jrpg.Editor.Tests/Battle/EnemyCatalogViewModelTests.cs` (160 → about 190), enemies built in the test:
- `EnemyCatalogViewModelTests: Choose_KnownId_ChoosesIt`
- `EnemyCatalogViewModelTests: Choose_UnknownId_KeepsTheChoice`
- `EnemyCatalogViewModelTests: ChosenKey_DuplicatedEnemy_IsItsTypedId`

## Verify by hand
Agents may run it (battlers are generated art). The script: "Database>Enemies...: opened", the idle loop playing in
the preview. In the editor: change an enemy's HP, Edit... beside Drop table, add a table, OK, pick it in the box, OK:
Ctrl+Z takes back the enemies' step, then the table's. Close the Enemies dialog during a loop: no `RenderTick` report.
