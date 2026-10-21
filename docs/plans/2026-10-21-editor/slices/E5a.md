# E5a. Every sheet image is listed

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Data changes"; "What exists" (24 of 34 images have no sheet).

- **E5a. Every sheet image is listed** (Data + Editor, about 120 lines)
  - **Goal:**
    - The sheet box lists every PNG in `content/art/tilesheets/`: set-up sheets by id ("grass_1"), the others by file name with "(not set up)" ("Grass3.png (not set up)"), sorted by file name.
    - Choosing an image that isn't set up shows it with no grid; the line under it says "Not set up yet". Clicks on it choose nothing.
    - **Known gap until E5b:** an image can't be set up yet.
  - **Data:** `Art/ArtFolders.TilesheetImagesIn(string artRoot)`: the PNG file names in the tilesheet folder, sorted ordinally.
  - **New types:**
    - `Tiles/SheetChoice` (record): one entry of the sheet box, an image file and its sheet if it has one.
  - **Changes:**
    - `Tiles/TileBrowserViewModel`: `SheetChoices` and `SelectedChoice` replace `Sheets` and `SelectedSheet` (`SelectedSheet` stays as the choice's sheet, null when not set up); the list is rebuilt when `Current.Tilesheets` changes (from E5b); `CellAt` returns null for an image not set up.
    - `Tiles/TileBrowserView`: draws an image without a sheet, with no marks.
  - **Existing tests this breaks:** `TileBrowserViewModelTests.Constructor_SheetsSortedById_ShowsTheFirst` becomes `Constructor_ImagesSortedByFile_ShowsTheFirst` (choices now follow file names); construction lines gain the image list.
  - **Tests:**
    - `ArtFoldersTests` (Data): `TilesheetImagesIn_RepositoryArt_ListsEveryPngSorted`.
    - `TileBrowserViewModelTests`: `SheetChoices_ImageWithoutSheet_IsListedNotSetUp`, `SelectCellAt_ImageNotSetUp_ChoosesNothing`.
  - **Verify by hand:**
    - [ ] The box lists all 34 images; 24 say "(not set up)"; choosing `Grass3.png` shows it without a grid.
