# WS6. Edit... shows a page on a record; the dialog goes

**Stage 3**, its last slice. Needs WS5.

**Goal:** an Edit... that reaches another catalog shows that catalog's page on the record (the Enemies page's Edit...
beside the Drop table box; a Skills page's strip Edit..., row 7, if it has one), the calling page applied first or the
move refused. The nested refresh (decision 602) and, with nothing left opening a page in a dialog, `CatalogDialog` and
`DialogBounds` go. Design: [design.md](../design.md), section 3 and the replacement mapping; decisions 928, 940.

## New types
None.

## Changes
- `Workspaces/DatabaseWorkspaceViewModel`: `ShowPageOnRecord(CatalogPage page, string key)`: as `ShowPage`, the page
  started on that key.
- `Shell/MainViewModel.Workspaces.cs`: `ShowDatabasePageOnRecord(string title, string key)`.
- `Shell/MainWindow.Database.cs`: the Enemies page's registration passes `editDropTable = tableId =>
  _viewModel.ShowDatabasePageOnRecord("Drop tables", tableId)`, the table id read before the call (decision 940: the
  Enemies page is detached after it); the Skills page's strip Edit... the same way with "Effect strips" and its file
  (check row 7's page for its name); `EditDropTables` and `OpenInDialog` go.
- `Battle/EnemyCatalogPage`: its constructor takes `Action<string> editDropTable` (was `Action<Window, string>`); the
  param doc says it shows the Drop tables page.
- `Battle/EnemyCatalogViewModel.TakeDropTables` and `Battle/EnemyEditingViewModel.RefreshDropTables` go (the restart
  on a draft change offers the new tables).
- Deleted: `Workspaces/CatalogDialog.xaml`, `.xaml.cs`, `Workspaces/DialogBounds.cs` (grep `CatalogDialog` and
  `DialogBounds` across `src/` first: a row's plan may have opened a page in a dialog from World mode; if so, keep
  them and say so in the Done note); `tests/Jrpg.Editor.Tests/Battle/EnemyDropTableRefreshTests.cs` (100 lines: what it
  guarded, a box keeping its choice when tables are inserted, the restart now covers).
- ADR 0045: its amendment B says it is retired by ADR 0060 (one line); `docs/map.md`: the Battle, Loot and
  `Workspaces` rows.

## Tests
`tests/Jrpg.Editor.Tests/Workspaces/DatabaseWorkspaceLeavingTests.cs` (WS3's, about 150 → 190), with an enemies page
built in the test over `EnemyEditingViewModel` and a drop tables page over `DropTableEditingViewModel`:
- `DatabaseWorkspaceLeavingTests: ShowPageOnRecord_Key_ShowsThePageOnIt`
- `DatabaseWorkspaceLeavingTests: ShowPageOnRecord_ShownPageWithProblem_KeepsTheShownPage`
- `DatabaseWorkspaceLeavingTests: DraftChanged_DropTableAdded_TheEnemiesPageOffersIt`

## Verify by hand
Agents may run it. In the editor: on the Enemies page, change an enemy's HP, press Edit... beside Drop table: the Drop
tables page on that table, the HP change applied (Ctrl+Z later takes it back); add a table; Enemies in the list: the
same enemy, the new table in the box. An enemy with an empty id typed: Edit... refused, the bar naming it. The script:
no `ERROR`, no binding-error line.
- **Note from WS1:** the Skills page landed without an Edit... button (its strip is a box), so the Enemies page's
  Edit... beside the Drop table box is the only one reaching another catalog; `CatalogDialog` and `DialogBounds` have
  no user outside `Shell/MainWindow.Database.cs` (grepped in WS1).
- **Note from WS5:** `OpenInDialog` is gone; the Enemies page's registration is in
  `MainWindow.RegisterPagesOnTheAnimationClock` (`EditDropTables` then `RefreshDropTables` on the page's own edited
  catalog, now redundant since the dialog's OK restarts the shown session). `EnemyCatalogPage.HostWindow`'s comment
  already says the main window owns what it opens in Database mode.
- **Done, with deviations:** no Skills strip Edit... (WS1's note), so the Enemies page's is the only caller;
  `CatalogDialog` and `DialogBounds` had no user left in `src/` and went. The tests went to a sibling,
  `DatabaseWorkspaceOnRecordTests` (92 lines; `DatabaseWorkspaceLeavingTests` was at 273), plus
  `ShowPageOnRecord_NoTableNamed_ShowsTheFirstTable` (an enemy dropping nothing) and
  `ShowPageOnRecord_TypedName_AppliesTheShownPageFirst`, and
  `MainViewModelDatabaseWorkspaceTests.ShowDatabasePageOnRecord_DropTables_ShowsThePageOnTheTable` (151; review: on
  vault, a table the test adds after crypt, so the content's only table can't pass it by chance); the six
  `EnemyDropTableRefreshTests` went, so the total stays 7457. `ShowPage`, `ShowPageOnRecord` and `TryClose` share
  `TryLeaveShown` (apply, then remember the chosen key; its third copy). `MainWindow.TryApplyShownPage` went with
  `EditDropTables`, its only caller. `EnemyCatalogViewModel.DropTableChoices` is a plain list now (nothing inserts) and
  `_content` readonly; the Edit... tooltip says it shows the Drop tables page; `IEditedCatalog`'s summary no longer
  names the dialog. ADR 0045 marks amendment B retired (header line and under B), ADR 0060 and the ADR index say it is
  done, ADR 0060's decision 928 naming the Enemies page's Edit... the only one (review).
  `DatabaseWorkspaceViewModel.cs` 217 lines, `MainWindow.Database.cs` 96, `MainViewModel.Workspaces.cs` 143,
  `EnemyCatalogViewModel.cs` 158, `EnemyCatalogPage.xaml.cs` 178, `EnemyEditingViewModel.cs` 22.
