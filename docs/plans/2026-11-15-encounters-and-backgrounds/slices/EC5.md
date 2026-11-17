# EC5. The Encounters page

Editor; about 170 lines with tests (about 140 of XAML, code-behind and wiring, 3 tests). Needs EC3, EC4 and the mode
bar plan's stage 1 (its page recipe, `Workspaces/CatalogDialog`, `Shell/DatabaseMenu.xaml`). Decisions 905, 906.

## Goal
**Database > Encounters** shows a page (built on the mode bar plan's recipe, its WS12 file): encounters left with
**Add** and **Remove**; right, the chosen encounter's **Id** (editable while new), its **Enemies** in slot order with an
enemy box, **Add**, **Remove**, **Up** and **Down**, its **Background** and **Music** boxes. The bar's Apply records one
undo step; Save all writes `encounters.json`.

## New types
- `Encounters/EncounterPage` (`UserControl`, XAML + code-behind): the page, per the recipe (no OK, Cancel or problem
  line; theme resources only from `EditorTheme.xaml`), laid out as the shops' (a list box of `Label`s; the chosen
  encounter's panel under the shared `EnabledWhenChosen` style). The slot list shows each row's `Name` with its slot
  number from 1 ("1. Goblin"), a note under it: "Slot 1 stands front left; each next slot a row up and to the right."
  The Background box's tooltip: "(the map's) fights on the background the map names in Map > Settings...; a file here
  wins on every map (a boss's arena). The folder also holds the title's and the menu's pictures."

## Changes
- Page registration (the recipe's steps 3 and 4): one `CatalogPage.Of("Encounters", <the Battle group>, editing)` line
  in `Shell/MainViewModel.Workspaces`, the editing view model built with a `ReadEncounterChoices()` (in
  `Shell/MainViewModel.Database`) that reads, when the page starts, the draft's enemies as `EnemyChoice`s by name (then
  id), `ArtFolders.ImagesIn(ArtRoot, ArtFolders.Backgrounds)` and `AudioFolders.TracksIn(_audioRoot)`; one
  `Register<...>` line in `Shell/MainWindow.Database`.
- `Shell/DatabaseMenu.xaml`: an "Encounters" entry after Enemies, bound to `ShowDatabasePageCommand` as its neighbours,
  tooltip "Each encounter's enemies in their places, its own background or the map's, and its music; Map > Settings...
  picks which encounters a map rolls."; the Enemies entry's tooltip loses "encounters in encounters.json".
- `.claude/scripts/open-editor-windows.ps1`: "Database>Encounters" joins the default list (a page: "shown as a page").
- `docs/map.md`: the `Encounters` row names the page and how it shows.

## Tests
- `MainViewModelEncounterTests` (Editor, new; globbed: no such file; the editor test content as the other
  `MainViewModel*Tests` load it):
  - `ReadEncounterChoices_Enemies_OfferedByNameThenId` (two enemies built with `with`, names out of id order)
  - `ReadEncounterChoices_ImageAddedToTheFolderAfterStart_IsOffered` (a PNG copied into the test's backgrounds folder
    after the view model was built; the choices are read when the page starts)
  - `EncounterPage_Registered_InTheBattleGroup` (the page list holds "Encounters" under its group, as the recipe's
    registration tests do)

## Verify by hand
1. `.claude/scripts/open-editor-windows.ps1`: "Encounters" shown as a page, no `ERROR` line, no binding-error line.
2. **Database > Encounters**: `goblin_chief`, `goblin_trio`, `stone_beetle`, `training_dummy` in id order;
   `goblin_trio` shows "1. Goblin", "2. Goblin", "3. Goblin", Background **(the map's)**, Music **(battle theme)**;
   `goblin_chief`'s music `boss/loop.wav`.
3. **Add**: an empty id, the bar's problem line names it, Apply grey. Type `test_pair`, add two goblins and a stone
   beetle, choose the second goblin, **Down**: that row moves (the first goblin stays first). Background `Forest.png`.
   **Apply**, **Ctrl+S**: `encounters.json` gains `test_pair` sorted by id, its enemies one per line,
   `backgroundFile` written, no `musicFile`. **Ctrl+Z**, **Ctrl+S**: `git status` clean.
4. **Remove** is grey on `goblin_trio`, lit on a new one.
5. EC4's refusal: add `test_pair` again, **Apply**; show World mode, open `milestone_outside`, **Map > Settings...**,
   tick `test_pair`, **OK**; switch to `milestone` (the map list) and **Ctrl+Z**: the status band reads "Can't undo:
   encounter test_pair is rolled on milestone_outside." Close the editor with **Don't save**.
6. Capture the page (no Mana Seed art in it).
- **Note from BG1:** the mode bar's WS1-WS6 hadn't landed when row 10 started (`Workspaces/` holds `IEditedCatalog`,
  `CatalogDialog`, `DialogBounds`): unless WS2 has landed by then, host the page in `CatalogDialog` through
  `MainWindow.Database.OpenInDialog`, as WD2-WD8 and IU15b did, and declare no `ICatalogEditing<T>`.
- **Note from BG1:** `goblin_chief` (row 8's boss) now uses the map's background like the others; decision 892's boss arena
  has no content yet, so a hand check giving an encounter its own background picks one from the folder.
