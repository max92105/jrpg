# WD8. The Character sprites page

**Stage 2**, its last slice. Recipe: [page-recipe.md](../page-recipe.md), "A window to convert" (step 0 first: row 5's
cells and mirrors have changed this window since 2026-11-15).

**Goal:** the Character sprites window becomes a page in `CatalogDialog`, opened from NPCs > Character sprites... as
today.

## New page
- `Sprites/FieldSpritePage` (user control): the window's layout and code-behind without OK and Cancel; what `OnClosed`
  let go of (its render tick, a shared view's DataContext) is let go on `Unloaded`; its import's file dialog owner is
  `Window.GetWindow(this)`.

## Changes
- `Sprites/FieldSpriteCatalogViewModel`: implements `IEditedCatalog` (`ChosenKey` = `Chosen?.File`, `Choose` by file,
  a missing image's record included).
- `Shell/MainWindow.FieldSprites.cs`: the opener through `OpenInDialog` (moved into `MainWindow.Database.cs` with the
  others; the file deleted), under the title "Character sprites", called by the NPCs entry's handler.
- Deleted: `Sprites/FieldSpriteWindow.xaml`, `.xaml.cs`. `docs/map.md`: the Sprites row; grep "window" in the Trade,
  Quests, Loot, Battle, Cinematics and Sprites rows for a converted window's old wording.

## Tests
New sibling `tests/Jrpg.Editor.Tests/Sprites/FieldSpriteCatalogViewModelChoiceTests.cs` (the base file is at 242),
images built in the test as the base file's:
- `FieldSpriteCatalogViewModelChoiceTests: Choose_ImageWithoutRecord_ChoosesIt`
- `FieldSpriteCatalogViewModelChoiceTests: Choose_UnknownFile_KeepsTheChoice`
- `FieldSpriteCatalogViewModelChoiceTests: ChosenKey_ChosenImage_IsItsFile`

## Verify by hand
**The developer's** (the page shows Mana Seed sheets): NPCs > Character sprites... opens at the main window's size;
change a facing's cells, OK; New NPC's preview walks with it; Ctrl+Z takes it back. Agents run only the script and
read its lines: "NPCs>Character sprites...: opened", no `ERROR`, no binding-error line.
- **Done, with deviations:** only the tick goes on `Unloaded` (zoom, view model: the page's own); third tick copy as
  `RenderTick.RunWhileLoaded`, 2 tests; `Choose_RecordWithoutImage_ChoosesIt` added; tests' `window` renamed `sprites`;
  `CanEditFieldSprites` kept (the NPCs entry reads it). Page 180 + 181 lines, `MainWindow.Database.cs` 168.
