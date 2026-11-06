# MS2. Usage checks read every open map

Builds on design.md, "Key decisions" 2 and the folder edges (`Catalogs` reads no editor folder); decision 502. Needs MS1.

## Goal
With several maps open, every check of what uses a tile, sheet, object or quest reads each open map as it is now, unsaved changes included, in place of its saved version (a new map is read too): an undo that would remove a record a hidden open map uses is refused naming that map; Remove object and Remove sheet are refused the same way; the re-cut warning names it. The purge (Remove sheet on a missing image) waits while any open map has changes and rebuilds every open map from what it wrote.

## New types
None.

## Changes
- `Catalogs/CatalogUsage`: `MapLayout? openLayout` becomes `IReadOnlyCollection<MapLayout> openLayouts` in `UsersOfTile`, `OfSheet`, `UsersOfObject`, `UsersOfQuest` and the private `LayoutsOf` (the saved maps whose id no open layout has, then every open layout); its summary and param docs say "the open maps' layouts, unsaved changes included; empty when none is open".
- `Maps/OpenMaps` (MS1): `public IReadOnlyCollection<MapLayout> Layouts` (each open map's layout now); `public void ReplaceFrom(ContentCatalog written)` (done in MS1, keeping a map with unsaved changes as it is: every open map rebuilt from the written content; a map the written content lacks is dropped; the purge runs only with nothing dirty, so nothing unsaved is lost).
- Providers, each now `Func<IReadOnlyCollection<MapLayout>>` from `OpenMaps.Layouts`:
  - `Shell/CombinedHistory(CatalogDraft catalog, OpenMaps openMaps)`: its five calls pass `openMaps.Layouts`.
  - `Objects/ObjectPlacingViewModel(CatalogDraft catalog, LayerChoiceViewModel layer, OpenMaps openMaps)`: its browser's provider.
  - `Objects/ObjectBrowserViewModel` and `Tiles/TileBrowserViewModel`: the provider's type.
  - `Tiles/TilePaintingViewModel(CatalogDraft catalog, LayerChoiceViewModel layer, TileBrowserViewModel browser)`: `MainViewModel` builds the browser (with the images, the yes/no question and `() => _openMaps.Layouts`); from 4 parameters to 3.
  - `Tiles/SheetRemoval(CatalogDraft catalog, TileBrowserViewModel browser, OpenMaps openMaps, ContentWriter writer)`: the usage check reads `openMaps.Layouts`; `AskPurge` says "Save first" while any open map is dirty (`openMaps.Dirty.Count > 0`) or the catalogs are.
- `Shell/MainViewModel.cs`: the constructor lines building these pass `_openMaps` (same lines, one longer each); the `Content.Rewriting` lambda becomes `(_, written) => ShowRewritten(written)` (same line), `ShowRewritten` in the MapSwitching partial calling `_openMaps.ReplaceFrom(written)` and showing the rebuilt shown map.
- Test call sites, mechanical: the 17 `CatalogUsage` calls in `CatalogUsageTests`, `CatalogUsageQuestTests` and `CatalogDraftRemovalTests` pass `[]` for `null` and `[layout]` for a layout; the 16 constructions of the changed view models (`ObjectPlacingViewModelTests`, `ObjectBrowserViewModelTests`, `ObjectBrowserRemovalTests`, `SheetRemoval*Tests`, `TileBrowser*Tests`, `TilePaintingViewModelTests`, `WalkabilityMarkingTests`) take an empty `OpenMaps` or `() => []`.

## Tests
- `CatalogUsageTests: UsersOfObject_TwoOpenLayouts_NamesBoth` (each open layout read in place of its saved version).
- `CatalogUsageTests: UsersOfTile_OpenNewMapNotSaved_NamesIt` (an open layout whose id no saved map has).
- `MainViewModelOpenMapsTests: Undo_ObjectPlacedOnAHiddenUnsavedMap_IsRefusedNamingIt` (compose an object, place it on map A unsaved, open B, Ctrl+Z: refused naming A).
- `MainViewModelOpenMapsTests: RemoveObject_PlacedOnAHiddenUnsavedMap_IsRefused` (Objects panel's Remove).
- `OpenMapsTests: Layouts_MapPainted_HoldsTheUnsavedTile`.
- `OpenMapsTests: ReplaceFrom_WrittenContent_RebuildsEveryOpenMapClean`.
- `SheetRemovalMissingImageTests: RemoveSheet_AHiddenOpenMapUnsaved_AsksToSaveFirst`.

Size: about 45 lines of code changed, 33 mechanical test sites (about 40 lines), 7 tests (about 90): about 175.

## Map
`docs/map.md`, Editor `Catalogs` row: `CatalogUsage` reads every open map's unsaved layout.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5).
2. Objects > New object...: make a small object `test_box`; place it on map A. Don't save.
3. Open map B. Press Ctrl+Z until the next undo would remove `test_box`: the status bar says "Can't undo: object test_box is used on map A." and the object stays.
4. Objects tab: choose `test_box`, press Remove: refused, naming map A.
5. Open map A, right click the placed `test_box` (removed); in the Objects tab choose `test_box` and press Remove: it leaves the list.

- **Done, with deviations:** `OpenMaps.ReplaceFrom` and `ShowRewritten` were already done in MS1 (its review). `RemoveSheet_AHiddenOpenMapUnsaved_AsksToSaveFirst` went to `MainViewModelPurgeTests`, through the editor's own wiring, in place of `SheetRemovalMissingImageTests`. Review: two tests through the editor reach the re-cut warning and Remove sheet with a hidden unsaved map (`MainViewModelOpenMapsTests`, now 296 lines: MS3's tests go to a sibling file); two comments say "open maps".
