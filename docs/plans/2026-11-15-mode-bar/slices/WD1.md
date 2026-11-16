# WD1. The Database menu in its own file

**Stage 1**, right after row 6 and before row 7's first window slice (decision 936). A pure move.

**Goal:** the Database menu leaves `Shell/MainWindow.xaml` for `Shell/DatabaseMenu.xaml`, so rows 7, 9, 10 and 12 add
their entries there and the main window's XAML stops growing (row 9's decision 890, done here: IU8's step 0 is then
skipped). Every entry still opens its window as today. Design: [design.md](../design.md), sections 2 and 3 (stages 1
and 2); decision 938.

## New types
- `Shell/DatabaseMenu` (`MenuItem`-rooted XAML with code-behind holding only `InitializeComponent`): the Database menu's
  entries, their headers, tooltips and `IsEnabled` bindings unchanged, each with `Tag` set to its title ("Quests",
  "Shops", "Drop tables", "Effect strips", "Cinematics", "Enemies").

## Changes
- `Shell/MainWindow.xaml` (recount first; 294 on 2026-11-15): the Database menu's lines become one
  `<local:DatabaseMenu MenuItem.Click="OnDatabaseMenuClicked" />` (about -6 lines).
- `Shell/MainWindow.Database.cs`: `OnDatabaseMenuClicked(object sender, RoutedEventArgs e)` takes the clicked entry
  (`e.OriginalSource is MenuItem { Tag: string title }`) and runs the opener registered under that title, from a
  `Dictionary<string, Action>` filled in the constructor with today's six handlers (renamed `OpenShops`, `OpenQuests`
  ...; `MainWindow.QuestEditing`'s moves into the dictionary from its own file). A title with no opener throws
  `InvalidOperationException` naming it (a typo in the XAML; the smoke script would show it as `ERROR`). The class
  summary says it dispatches the Database menu.
- `.claude/scripts/open-editor-windows.ps1`: nothing (the paths are the menu's text).
- `docs/map.md`: the Shell row names `DatabaseMenu`.

## Tests
None new: the entries' behavior is unchanged and window code isn't tested headless. The guard is the smoke script.

## Verify by hand
Agents may run it: `dotnet build src/Jrpg.Editor`, then `open-editor-windows.ps1`: the six `Database>...` entries
"opened" with the same screenshots as before, no `ERROR`, no binding-error line (EW0). In the editor, each Database
entry's tooltip shows.
- **Done, with deviations:** `DatabaseMenu` sets the theme's `MenuItem` style itself (an implicit style skips a
  subclass); the openers' dictionary is built by `CreateDatabaseOpeners`; `OpenQuests` stays in `.QuestEditing` (WD3
  deletes the file). The smoke script opened all six entries and caught the test error, no `ERROR`.
