# WS5. The Database view hosts every page

**Stage 3.** Needs WS4.

**Goal:** the Database mode shows its grouped page list on the left and the shown page under a bar (title, Apply,
Revert, the problem line, the refusal, the note); every page built in stages 1-2 and by rows 7-10 shows there; the
Database menu shows pages instead of opening dialogs; the smoke script reaches pages. Design:
[design.md](../design.md), sections 1 and 3 (key decisions); decisions 917, 926, 929, 930, 933, 938, 940.

## New types
- `Workspaces/CatalogGroup`: the list's headings, in order (`Story`, `Battle`, `Items`, `Trade`, `Characters`).
- `Workspaces/DatabasePageViews`: builds the view of a shown page from its edited catalog, by the catalog's type:
  `Register<TEdited>(Func<TEdited, FrameworkElement> build)`, `FrameworkElement ViewOf(IEditedCatalog edited)` (throws
  `InvalidOperationException` naming the type when none is registered).

## Changes
- `Workspaces/CatalogPage`: `Of` takes a `CatalogGroup` (its only caller, `MainViewModel.Workspaces`, changes); `Group`.
- `Workspaces/DatabaseWorkspaceViewModel`: `CatalogPage PageTitled(string title)` (throws on an unknown title) and
  `bool HasPage(string title)`.
- `Workspaces/DatabaseWorkspaceView` (replaces the placeholder): a 200 px left column, a `CollectionViewSource` grouped
  on `Group` with each page a button running `ShowPageCommand`, the shown one lit as the mode bar's; the bar docked on
  top (the page's title, Apply, Revert, `Session.Edited.Problem` and `Refusal` in `ProblemTextBrush`, the note in
  `MutedTextBrush`); a `ContentControl` page host. Code-behind: a `PageViews` property; on `Session` changing, the
  host's content is `PageViews.ViewOf(Session.Edited)` or null, the built page's `AutomationProperties.Name` set to
  "{Title} page" (decision 933); when the old page held the keyboard focus, the new one takes it
  (`MoveFocus(new TraversalRequest(FocusNavigationDirection.First))`, decision 940). Only `EditorTheme.xaml` resources.
- `Shell/MainViewModel.Workspaces.cs`: `[RelayCommand(CanExecute = nameof(HasDatabasePage))] ShowDatabasePage(string
  title)`: shows Database, then the page (a refused move leaves things as they are).
- `Shell/MainWindow.Database.cs`: each opener's `buildPage` lambda moves into `CreatePageViews()`, one
  `Register<TEdited>` line per page, set on the Database view in the constructor (`MainWindow.xaml.cs` +1); the
  openers and `OnDatabaseMenuClicked` go, except `EditDropTables` and `OpenInDialog` (the Enemies page's Edit... until
  WS6). The class summary says it registers the pages.
- `Shell/DatabaseMenu.xaml`: every entry binds `ShowDatabasePageCommand` with its title, "..." dropped from the
  headers; the `MenuItem.Click` hook leaves `MainWindow.xaml`. If this slice is split by entry, the routed handler
  skips an entry whose `Command` is `ShowDatabasePageCommand` (decision 938) until the last moves.
- `Shell/MainWindow.xaml`: NPCs > Character sprites binds `ShowDatabasePageCommand` with "Character sprites" (in place).
- `.claude/scripts/open-editor-windows.ps1`: when no dialog opened and the main window holds an element named
  "$itemName page", report "$entry: shown as a page, <file>" with the main window's screenshot; the default list runs
  `Map>Settings...` and `NPCs>Character sprites` (and any other World-menu entry) before the first `Database>` entry,
  names the pages without "..."; the header comment says pages are reached through the same entries and that the
  Character sprites page's screenshot is the developer's.
- `Shell/ControlsHelpWindow.xaml`: a Database section: "Database mode: the catalogs on the left; Apply records the
  page as one Ctrl+Z step, Revert drops what was typed. Leaving the page, Ctrl+Z, Ctrl+Y, Save and closing apply it
  first; a page with a problem stays until it is fixed or reverted."
- `docs/map.md`: the `Workspaces` row.

## Tests
New file `tests/Jrpg.Editor.Tests/Workspaces/DatabasePageViewsTests.cs`, run on `StaThread` (a `TextBlock` factory
built in the test):
- `DatabasePageViewsTests: ViewOf_RegisteredCatalog_BuildsItsView`
- `DatabasePageViewsTests: ViewOf_UnregisteredCatalog_ThrowsNamingIt`

`tests/Jrpg.Editor.Tests/Shell/MainViewModelWorkspaceTests.cs` (about 130 → 165):
- `MainViewModelWorkspaceTests: ShowDatabasePage_Shops_ShowsDatabaseOnTheShopsPage`
- `MainViewModelWorkspaceTests: ShowDatabasePage_ShownPageWithProblem_KeepsIt`
- `MainViewModelWorkspaceTests: ShowDatabasePageCommand_UnknownTitle_IsGreyed`

## Size
About 200 lines of code and XAML (the page builders moved), 50 of tests, 20 of script: about 260. Split point if
needed: the script and the help lines second.

## Verify by hand
Agents may run steps 1-5 (Shops, Quests, Enemies, Drop tables, Effect strips and Cinematics show no Mana Seed art):
1. `dotnet build src/Jrpg.Editor`, then `.claude/scripts/open-editor-windows.ps1`: every `Database>` entry "shown as a
   page", the World-menu entries first and "opened", no `ERROR`, no binding-error line (EW0). Look at the Shops
   screenshot: the grouped list, the bar, the page filling the window.
2. Run the editor. Database > Shops: Database mode on the Shops page. Click into a shop's name box, type, Ctrl+Z: the
   box takes back its own typing (WPF's), the page still shown and nothing applied. Click into the shop list, rename a
   shop in its box, click the list, Ctrl+Z: the old name; Ctrl+Y: the typed one; Ctrl+Z: the old name again. Rename it
   once more, then Ctrl+Y: the rename is applied and Redo greys (decision 920).
3. Add shop, leave its id empty: the problem line shows and Apply greys. Press World: refused, the refusal names the
   page; Ctrl+S: "Nothing was written."; Quests in the list: refused; Revert: the added shop is gone, World works.
4. Rename a shop, press World: the status shows "Unsaved changes"; close the editor: the close question lists the
   catalogs; Cancel. Ctrl+S: `content/shops.json` holds the name; put it back with Ctrl+Z, Ctrl+S.
5. Enemies: the idle loop plays; switch to Effect strips: it plays; World and back: the same page and record, focus in
   the page.
6. **The developer's:** the Character sprites page (Mana Seed art), through NPCs > Character sprites and the list.
- **Note from WD7:** the Cinematics page's `Unloaded` stops the clock but leaves `Preview.IsPlaying` true; once a page
  is hidden and shown again, it comes back frozen mid-play: `Unloaded` should also call `Preview.Stop()` (WD7 review).
- **Note from WS1:** fourteen `Register<TEdited>` lines: the WD seven plus Skills, Effects, Statuses, Items, Affixes,
  Battle backgrounds and Encounters; the builders keep their callbacks (Items' and Cinematics' decoding at width,
  Enemies' `EnemyArtSources` and drop-table Edit..., Effect strips' three, Battle backgrounds' two, Character sprites'
  import). The list's groups and order are in WS3's note. The script: its default list already holds a three-part
  entry, `Database>Effect strips...>Strip from sheet cells...` (decision 996); as a page it must find that button in
  the main window, the tool still opening as a dialog. Captures: the Skills and Items pages show icons and Character
  sprites shows Mana Seed sheets, so their screenshots are the developer's; the header comment's list of windows agents
  may look at becomes a list of pages (Quests, Shops, Drop tables, Effect strips, Battle backgrounds, Cinematics,
  Enemies, Encounters). `Shell/MainWindow.xaml` is at 296 lines after WS1: the view's content goes in
  `DatabaseWorkspaceView`, never there.
- **Note from WS4:** once a page is typable, the unexpected-error box (ADR 0050) lists a Database page only after a
  refused move (decision 1009): valid typed changes not yet applied are not named there. Say so in the hand check, or
  have the line run `TryApplyShown` first if the developer wants it (his call).
