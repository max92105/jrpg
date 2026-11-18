# NG3. Database > New game: the page

Editor, about 40 lines plus about 110 of XAML. Needs NG2, NG2b, NG2c and row 11's mode bar (stage 3). Design: section 3;
decisions 942, 962, 968; the mode bar's WS12 recipe, steps 3-7 and 10.

## Goal
Database > New game shows the New game page under the Characters group: Leader choices (the list with Up, Down, Remove,
the member box and Add under it; a note "The leader screen offers them in this order."), Starting items (rows of item
and quantity, the same buttons), Starting gold, Start (map box, X, Y, facing), Player sprite (editable box over the
field sprite files). The bar shows Apply, Revert and the problem line.

**Recount first:** the files below on the day; one that would pass 300 gets its move in this slice (decision 962).

## New types
- `NewGame/NewGamePage` (`.xaml` + `.xaml.cs`, a `UserControl`): the New game page, four groups in two columns.
  Resources from `EditorTheme.xaml` only.

## Changes
- `Shell/MainViewModel.Workspaces.cs`: `CatalogPage.Of("New game", CatalogGroup.Characters, new
  NewGameEditingViewModel(content))` after Party members.
- `Shell/MainWindow.Database.cs`: `Register<NewGameViewModel>(newGame => new NewGamePage(newGame))`.
- `Shell/DatabaseMenu.xaml`: `<MenuItem Header="New game" Command="{Binding ShowDatabasePageCommand}"
  CommandParameter="New game" ToolTip="The leaders a new game offers, its starting items and gold, where it starts and
  the player's sprite." />`.
- `.claude/scripts/open-editor-windows.ps1`: `'Database>New game'` joins the default list.
- `docs/map.md`, Editor `NewGame` row: `NewGamePage`.

**Fallback (row 11 moved after this row):** `NewGame/NewGameWindow` (720 x 560) with OK, Cancel and the problem line, a
`Database > New game...` entry and its handler in `Shell/MainWindow.Party.cs`.

## Tests
None new: the page binds NG2's tested view model. Guard: the smoke script shows it with no `ERROR` line.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: "Database>New game: shown as a page", no `ERROR`, no binding-error
   line.
2. Database > New game: nine leaders in `new-game.json`'s order, the starting items with their quantities, gold 100,
   start `intro` at 2.5, 2.5 facing down, sprite `player.png`.
3. Move Sarxian to the top, Apply, Save all: `git diff content/new-game.json` shows only that move. Run the game: the
   leader screen offers Sarxian first. Ctrl+Z, Save all to restore.
4. Type `NaN` in X: the line names X and Apply greys. Type `999`: Data's "must be inside map" message. Revert.
5. Remove every leader: Data's "must list at least one character" and Apply greyed. Revert.
- **Note from CL1:** rows 6-11 landed as this plan expected, so this is a page in the Database mode, never a dialog:
  `Workspaces/CatalogDialog` is gone and the Fallback paragraph is void. The recipe is
  `docs/plans/done/2026-11-15-mode-bar/page-recipe.md` (what this file calls "the WS12 recipe"). Its lines on 2026-11-17:
  one `CatalogPage.Of(title, CatalogGroup.Characters, editing)` in `Shell/MainViewModel.Workspaces.cs`'s
  `CreateDatabaseWorkspace` (143 lines), the editing view model built there from its `content` parameter rather than
  as a new `XEditing` property (`MainViewModel.cs` is at 295 and each property costs a constructor line there); one
  `pageViews.Register<TEdited>(...)` in `Shell/MainWindow.Database.cs`'s `CreatePageViews` (96 lines; a page with
  pictures in `RegisterPicturePages`), and the map's Workspaces row count ("registers the fourteen") moves; the
  `Shell/DatabaseMenu.xaml` entry passes the title as `CommandParameter` (the recipe's "`Tag`" is stale);
  `CatalogGroup.Characters`' summary ("Character sprites.") gains the title; and
  `MainViewModelDatabaseWorkspaceTests.DatabaseWorkspace_ContentLoaded_ListsEveryPageInGroupOrder` gains it after the
  page before it, so "Tests: none new" still changes that one assertion. The smoke script's default list is its lines
  32-35.
- **Done, with deviations:** a page under Characters after Party members, as the CL1 note says (the Fallback void).
  The groups in two columns: the leader choices left, the starting items right with the starting gold, the start and
  the player sprite under them, in the problem line's order; "Field sprite" labels the player sprite's box. The
  backlog's `RowFocus.ChoosesRow` style line counts New game's copy (six pages). Sizes: `NewGamePage.xaml` 109,
  `.xaml.cs` 17, `MainViewModel.Workspaces` 152, `MainWindow.Database` 105, `DatabaseMenu.xaml` 26.
