# E3a. One live catalog in the editor

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Key decisions" 1 and "Folder dependencies"; ADR 0014.

- **E3a. One live catalog in the editor** (Editor, refactor, about 190 lines)
  - **Goal:** the editor's catalog lives in one observable place that the drawing, the tile images, the tile browser, the object browser, object placing and event editing read when they need it, instead of copies taken at startup. No visible change; it is the seam every later catalog change goes through (tiles from E3d, sheets from E5b, objects from E9, new objects from E12b and N3). Today's save is its first user: it puts the saved map in the draft.
  - **New types:**
    - `Catalogs/CatalogDraft` (`ObservableObject`): the content the editor works on, as loaded, with the maps saved since.
      - `CatalogDraft(ContentCatalog loaded)`.
      - `[ObservableProperty] ContentCatalog Current { get; private set; }`.
      - `AddSavedMap(MapDefinition map)`: the saved map replaces or joins `Current.Maps` (moved from `MainViewModel.Save`).
  - **Changes:**
    - `Shell/MainViewModel`: `Content` (`CatalogDraft?`, null when the content didn't load) replaces the `Catalog` setter; `Catalog` reads `Content?.Current`; `Save` calls `Content.AddSavedMap`.
    - `Tiles/TileImages`, `Maps/MapDrawing`, `Tiles/TileBrowserViewModel`, `Tiles/TilePaintingViewModel` take the `CatalogDraft` and read `Current` at each use.
    - `Objects/ObjectBrowserViewModel(CatalogDraft catalog)`: lists `Current.Objects` (rebuilding the list when `Current` changes comes with its first trigger, E9).
    - `Objects/ObjectPlacingViewModel(CatalogDraft catalog, LayerChoiceViewModel layer)`: reads `catalog.Current.Objects` at each use (hits, placing) instead of a dictionary taken at startup.
    - `Events/EventEditingViewModel(CatalogDraft catalog)`: reads `Current` when a dialog opens (object choices, `EventPresets.GuessOpenedObject`), so objects made in E12b and N3 can be swapped to.
    - `Shell/MainWindow`: builds `TileImages` and `MapDrawing` from `viewModel.Content`.
  - **Existing tests this breaks:** construction lines only, assertions unchanged: `TilePaintingViewModelTests`, `TileBrowserViewModelTests`, the fixture of `ObjectPlacingViewModelTests` (8 tests), `ObjectBrowserViewModelTests.Objects_RepositoryContent_SortedByCategoryThenId`, `EventEditingViewModelTests`.
  - **Tests:**
    - `CatalogDraftTests`: `AddSavedMap_NewMap_JoinsCurrent`, `AddSavedMap_ExistingMap_ReplacesIt`.
    - Otherwise the existing tests are the guard (a refactor slice).
  - **Verify by hand:**
    - [ ] Open, paint, place an object, edit an object's events, save, switch maps and back: same behavior as before.
- **Done, with deviations:** about 120 lines added, 50 removed. `MainViewModel.Catalog` stays as a read-only shortcut to `Content?.Current`, so the window's partial files and the settings and new-map dialogs keep reading it unchanged. The tile browser's sheet list and the object browser's object list are still taken when they are built (their rebuild comes with the first change to sheets, E5b, and objects, E9); every other reader reads `Current` at each use. Event editing builds its item and object choices when a dialog opens. Review follow-ups for E3d: a test that a reader sees a catalog change made after it was built, and `TileImages` must drop its cropped image of a tile whose cell or sheet changes.
