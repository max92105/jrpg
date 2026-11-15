# XP3. Database > Experience: the page

Editor, about 30 lines plus about 80 of XAML. Needs XP2 and row 11's mode bar (stage 3). Design: sections 1, 3;
decisions 942, 956, 962; the mode bar's WS12 recipe, steps 3-7 and 10.

## Goal
Database > Experience shows the Experience page under the Characters group: the max level at the top ("Max level 21"),
the rows in a virtualized scrolling list (label, amount box, total to reach, right-aligned), Add level and Remove last
level under it, the max level note. The bar shows Apply, Revert and the problem line. The Classes page's Levels tab
shown after an Apply reads the new number of levels (the workspace restarts a page from the draft).

**Recount first:** the files below on the day; one that would pass 300 gets its move in this slice (decision 962).

## New types
- `Progression/ExperienceTablePage` (`.xaml` + `.xaml.cs`, a `UserControl`): the Experience page over
  `ExperienceTableViewModel`. Resources from `EditorTheme.xaml` only.

## Changes
- `Shell/MainViewModel.Workspaces.cs`: `CatalogPage.Of("Experience", CatalogGroup.Characters, new
  ExperienceEditingViewModel(content))` after New game.
- `Shell/MainWindow.Database.cs`: `Register<ExperienceTableViewModel>(table => new ExperienceTablePage(table))`.
- `Shell/DatabaseMenu.xaml`: `<MenuItem Header="Experience" Command="{Binding ShowDatabasePageCommand}"
  CommandParameter="Experience" ToolTip="The experience each level needs; its length sets the max level." />`.
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Experience'` joins the default list.
- `docs/map.md`, Editor `Progression` row: `ExperienceTablePage`.

**Fallback (row 11 moved after this row):** `Progression/ExperienceTableWindow` (560 x 640) with OK, Cancel and the
problem line, a `Database > Experience...` entry and `OnEditExperienceClicked` in `Shell/MainWindow.Database.cs`.

## Tests
None new: the page binds XP2's tested view model. Guard: the smoke script shows it with no `ERROR` line.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Experience: shown as a page", no `ERROR`, no binding-error
   line.
2. Database > Experience: max level 21, twenty rows, the last total the sum of the table.
3. Remove last level twice: max level 19, and the note says saved members above 19 drop to it. Revert: nothing changed.
4. Type `50` in "Level 1 to 2", Apply, Save all: `rules.json`'s first entry is 50 and nothing else in the file changed.
   Run the game, win the first battle: the bar fills against 50. Ctrl+Z, Save all to restore.
5. Database > Classes, Levels tab: 21 rows; after Remove last level and Apply on the Experience page, back on Classes:
   20.
