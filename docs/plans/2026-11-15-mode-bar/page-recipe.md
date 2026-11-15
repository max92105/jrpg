# The Database page recipe

How a Database editor is written from stage 2 of the mode bar plan on: the six windows converted in WD3-WD8, and every
editor rows 7-10 and 12 add, built as a page from birth (decision 934). Other plans point here. Design: [design.md](design.md);
decisions 918, 926, 929, 930, 937, 938, 940.

A page is a user control over its catalog view model. Until WS5 it opens in `Workspaces/CatalogDialog` from
`Shell/DatabaseMenu.xaml`; from WS5 it is shown in the Database mode. Nothing in the page knows which host it is in.

## A new page (rows 7-10 and 12)
1. **The catalog view model** implements `Workspaces/IEditedCatalog`: `Problem` (raised on every typed change, Data's
   message included, as the windows' are), `ChosenKey` (the chosen record's id, or its file for an image catalog;
   null with none chosen) and `Choose(string key)` (an unknown key keeps the choice).
2. **The editing view model** keeps `Start()` and `Apply(edited)` (one `CatalogDraft.SetX` step); from WS2 on it
   declares `ICatalogEditing<TEdited>`.
3. **The page**, `<Feature>/<Catalog>Page.xaml(.cs)`, a `UserControl`:
   - no OK, Cancel or problem line: its host shows them;
   - every typed box binds `UpdateSourceTrigger=PropertyChanged`, so a key, a mode switch or Ctrl+Z that applies the
     page sees what was typed;
   - a binding reaching the page's own DataContext names the page type (`AncestorType=trade:ShopCatalogPage`), never
     `UserControl` (a page may hold user controls of its own) nor `Window`;
   - only per-frame work needs the `Loaded`/`Unloaded` pair: `RenderTick` started on `Loaded`, stopped on `Unloaded`;
     everything else is set up in the constructor;
   - dialogs it opens (file dialogs, the GIF options) are owned by `Window.GetWindow(this)` in stages 1-2 and by the
     main window once it calls into the shell (decision 940); a handler reads the values it needs before any call that
     may apply the page (`TryApplyShown`), since the page is detached after it;
   - resources only from `EditorTheme.xaml` (`ProblemTextBrush`, `EnabledWhenChosen`): `MainWindowStyles.xaml` keys
     are not found while the page's XAML loads;
   - sizes bound by `x:Static` read `double` constants.
4. **The menu:** one entry in `Shell/DatabaseMenu.xaml`, its `Tag` the page's title. Before WS5: an opener in
   `Shell/MainWindow.Database.cs` registered under that title, `OpenInDialog(title, editing.Start, editing.Apply,
   edited => new XPage(edited, lookups))`. From WS5: `ShowDatabasePageCommand` with the title, one `CatalogPage.Of(title,
   group, editing)` line in `MainViewModel.Workspaces` and one `Register<TEdited>` line in `MainWindow.Database.cs`.
5. **A command that changes the draft outside the page's own session** (an Edit... opening another catalog, an import
   into another catalog) goes through the shell: before WS6 a nested `CatalogDialog` (ADR 0045's amendment B); from
   WS6 `ShowDatabasePageOnRecord(title, key)`, which applies the page first.
6. **The smoke script:** one `Database>Title` entry in `open-editor-windows.ps1`'s default list, after every World-menu
   entry (decision 933).
7. **Tests:** the view model's file (a sibling past 250 lines) gets `Choose_KnownId_ChoosesIt`,
   `Choose_UnknownId_KeepsTheChoice` and `ChosenKey_..._Is...`, records built in the test; the page itself has none
   (no window in tests): its hand check is the script's line and a typed change kept through Apply or OK.
8. **A read-only page** (the skills, classes and enemies table): `Problem` always null, `Apply` records nothing; its
   slice adds `CatalogPage.IsReadOnly` (stage 3) or hides OK in the dialog, with its own tests.

## A window to convert (WD3-WD8)
0. **Re-read the window and recount** before starting: rows 3-10 change these windows (the Cinematics focus point,
   the Character sprites cells, later the Effect strips preview and the Enemies tactics); the slice's numbers are from
   2026-11-15.
1. Steps 1, 3, 4, 6 and 7 above.
2. `<Catalog>Window.xaml` → `<Catalog>Page.xaml`: the `Window` root becomes `UserControl`; `Title`, `Width`, `Height`,
   `Style`, `WindowStartupLocation`, `ShowInTaskbar` go (the title moves to the opener); OK, Cancel and the problem
   line go; `AncestorType=Window` becomes the page type; typed boxes get `PropertyChanged`.
3. Code-behind: `: Window` → `: UserControl`; `OnOkClicked` goes; `Closed` → `Unloaded` (with `Loaded` starting a
   `RenderTick`); `ShowDialog(this)` → `ShowDialog(Window.GetWindow(this))`.
4. The opener (`MainWindow.Database.cs`, `.QuestEditing`, `.FieldSprites`) becomes one `OpenInDialog` registration;
   its file goes when empty; the `HasXEditing` property and its menu binding go (the opener returns when the editing
   view model is null).
5. `docs/map.md`: the feature row names the page; grep "window" in the folder's comments for the old wording.
