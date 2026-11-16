# IU8. Database > Items opens the Items page

Editor, about 190 lines (XAML about 100, code about 40, tests about 50). Needs IU7, the mode bar's WD1 and WD2. Design:
design.md, sections 5 and 5.1; [page-recipe.md](../../2026-11-15-mode-bar/page-recipe.md), "A new page"; decisions
877, 879, 889, 890.

## Goal
Database > Items opens the Items page in `CatalogDialog` over the draft: the list left with its six buttons, the chosen
item's general fields right (the icon box offering the icons folder's PNGs with the picture beside it, a note when the
file is missing); the dialog shows the title "Items", the problem line, OK greyed by it, and Cancel. OK applies the
session as one undo step; Ctrl+Z after it undoes the whole session. The category forms come in IU10-IU13: until then an
equipment, a unique or a consumable shows its general fields only.

## New types
- `Editor/Items/ItemCatalogPage.xaml(.cs)` (a `UserControl`): the Items page (design.md, 5.1), built on the recipe's
  step 3: no OK, Cancel or problem line; every typed box `UpdateSourceTrigger=PropertyChanged`; a binding to the page's
  DataContext through `AncestorType=items:ItemCatalogPage`; resources only from `EditorTheme.xaml` (`EnabledWhenChosen`,
  `ProblemTextBrush` for the icon note); sizes bound by `x:Static` read `double` constants; no `Loaded`/`Unloaded` pair
  (no per-frame work). Its constructor takes the view model and the icon picture's loader.

## Changes
- `Editor/Items/ItemCatalogViewModel`: implements `Workspaces/IEditedCatalog`: `ChosenKey` (the chosen item's id, null
  with none chosen or while a new one has no id), `Choose(string key)` (an unknown key keeps the choice); `IconFiles`
  (read once when the session starts) and `MissingIconNote` on the chosen item ("Weapon.png isn't in art/icons: the game
  will refuse to start until it is.", decision 442), recomputed when the chosen item or its icon box changes.
- `Editor/Items/ItemEditingViewModel`: takes `Func<IReadOnlyList<string>> iconFiles`; declares
  `ICatalogEditing<ItemCatalogViewModel>` if the mode bar's WS2 has landed (else WS2 adds it, as for every page).
- `Editor/Shell/MainViewModel.Database`: `ItemEditing`; `MainViewModel.cs` sets it where the others are set (285 lines on
  2026-11-15, recount; +1), with `() => ArtFolders.ImagesIn(ArtRoot, ArtFolders.Icons)`.
- `Editor/Shell/DatabaseMenu.xaml`: "Items..." with `Tag="Items"` after Shops, its tooltip "Base items, equipment,
  uniques and materials: icons, stacks, prices, effects, bonuses and fixed affixes.".
- `Editor/Shell/MainWindow.Database.cs`: the opener registered under "Items", `OpenInDialog("Items", editing.Start,
  editing.Apply, edited => new ItemCatalogPage(edited, LoadIcon))` (returning when `ItemEditing` is null), the loader
  `SpriteImageFile.LoadFrom(artRoot, ArtFolders.Icons, file)`, nothing for a missing or unreadable file (decoded only
  after `PngSize.IsReadable`). From WS5 the recipe's two page lines replace it (row 11's work).
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Items...'` in the default `-Windows`, after every World-menu
  entry (decision 933).

## Tests
- `Editor.Tests/Items/ItemCatalogViewModelTests` (IU7's): `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`,
  `ChosenKey_NewItemWithoutId_IsNull`, `MissingIconNote_IconNotInTheFolder_NamesIt`,
  `MissingIconNote_IconTypedThatExists_IsCleared` (the note shown, then the box changed),
  `MissingIconNote_OtherItemChosen_IsRecomputed`.
- `Editor.Tests/Items/ItemEditingViewModelTests` (IU7's): `Start_ReadsTheIconFilesEachTime` (the lookup called again on
  a second `Start`).

## Verify by hand
- `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1`: no `ERROR` line; the
  Items dialog's capture (generated icons only, no Mana Seed art) shows the list and the fields.
- Run the editor (Debug), Database > Items...: the list starts with the consumables; choose Bronze Sword: its icon shows
  and Max stack is hidden; type 120 in Price: "Sells for 60 (50 % share)"; clear Price on Potion: the dialog's problem
  line names the shop selling it, OK greys; put 20 back, OK. Ctrl+Z: Bronze Sword's price is back. No binding-error
  line in the Output window.
- Add unique, type the id `test_band`, name "Test Band", OK, Save all: `items.json` gains `test_band` with
  `"isUnique": true` in id order; its category shows "Unique" and can't be changed. Ctrl+Z, Save all: it's gone.
- **Note from IU6:** until IU13, a unique's "Sells for" line is the share of its base price only, not of its piece
  value with the fixed affixes' gold: don't read it as the shop's figure at the hand check.
- **Note from IU7:** the four Add buttons bind `AddItemCommand` with `CommandParameter="{x:Static items:ItemCategory.X}"`
  (the toolkit's `RelayCommand<ItemCategory>` throws on a string parameter); the others are `DuplicateCommand` and
  `RemoveItemCommand`; `Problem` is already raised when `SelectedItem` changes (the chosen item's problem comes first).
