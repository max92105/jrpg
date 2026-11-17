# IU15b. The Affixes page and Database > Affixes

Editor, about 110 lines (the page's XAML about 65 and code about 16, the wiring about 12, tests about 30). IU15's
split point: IU15 landed the list (`Loot/AffixCatalogViewModel`), `Loot/AffixEditingViewModel`,
`CatalogDraft.SetAffixes` and `CatalogProblems.OfAffixes`. Needs IU15, the mode bar's WD1 and WD2. Design: design.md,
sections 2.4 and 5.2; [page-recipe.md](../../2026-11-15-mode-bar/page-recipe.md); decisions 884, 885, 890.

## Goal
Database > Affixes opens the Affixes page in `CatalogDialog`, as IU15's goal says: the list left with Add, Duplicate
and Remove; the chosen affix's Id, Stat, Min, Max, Weight, Gold value and "Rolls on" with a box per equipment kind
right; the dialog's problem line is `AffixCatalogViewModel.Problem`; OK is `AffixEditingViewModel.Apply`.

## New types
- `Editor/Loot/AffixCatalogPage.xaml(.cs)` (a `UserControl`): the page (design.md, 5.2) on the recipe's step 3: boxes
  on `PropertyChanged`, `AncestorType=loot:AffixCatalogPage`, resources from `EditorTheme.xaml` only, the kind boxes in
  two columns of four, sizes from `double` constants.

## Changes
- `Editor/Loot/AffixCatalogViewModel`: implements `Workspaces/IEditedCatalog`: `ChosenKey` (the chosen affix's id;
  null with none or while a new one has no id) and `Choose(string key)` (an unknown key keeps the choice); its summary
  says so.
- `Editor/Shell/MainViewModel.Database`: `AffixEditing`; `MainViewModel.cs` sets it (+1; 290 lines on IU15).
- `Editor/Shell/DatabaseMenu.xaml`: "Affixes..." with `Tag="Affixes"` after Drop tables, its tooltip "The affixes
  dropped gear may roll: stat, range, gear kinds, weight and gold value.".
- `Editor/Shell/MainWindow.Database.cs`: `["Affixes"] = OpenAffixes`, `OpenInDialog("Affixes", editing.Start,
  editing.Apply, edited => new AffixCatalogPage(edited))`, returning when `AffixEditing` is null.
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Affixes...'` in the default `-Windows`, after every World-menu
  entry.
- `docs/map.md`: the Loot editor row names the page and the catalog view model's `IEditedCatalog`.

## Tests
- `Editor.Tests/Loot/AffixCatalogViewModelTests` (or a sibling past 250 lines): `Choose_KnownId_ChoosesIt`,
  `Choose_UnknownId_KeepsTheChoice`, `ChosenKey_NewAffixWithoutId_IsNull`.

## Verify by hand
IU15's "Verify by hand" list, all of it: the smoke script's line (no `ERROR`, the Affixes capture showing the list and
the boxes), `power`'s Max typed 4 with no problem and saved, Ctrl+Z, the `test_ward` affix offered by the Items page's
Add affix, no binding-error line. Also: `power` moved to -5..-1 shows the items.json line for `chiefs_cleaver` and greys
OK.
- **Done, with deviations:** the page is 74 lines of XAML (16 of code); its sizes are typed in the XAML as the Drop
  tables page's, since no code reads one (no `x:Static`), and it needs no `AncestorType` binding: the right side reads
  only the chosen affix. The `Choose` tests went to the sibling `AffixCatalogViewModelChoiceTests` (61 lines; the list's
  file would have passed 250), with two more: `ChosenKey_AffixChosen_IsItsId`, `ChosenKey_NothingChosen_IsNull`.
  `AffixCatalogViewModel.cs` 97, `MainViewModel.cs` 291. Tests: 7072 (5 new). The smoke script's run had no `ERROR`.
