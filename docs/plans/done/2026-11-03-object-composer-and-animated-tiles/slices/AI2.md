# AI2. Tiles > Import animations...

Builds on design.md, "Key decisions" 10, and decisions 461, 463 and 469.

## Goal
Tiles > Import animations... opens a file dialog (`.tsx` files); the chosen file's sheet is the set-up sheet whose image has the `.tsx`'s image file name (case ignored); the import is refused, with a status line saying why, when no sheet matches, when the cut differs from the sheet's (tile size, margin or spacing), when the `.tsx` has an animation of another shape (AI1's refusal), or when it has none. Otherwise every animated cell becomes a tile where it isn't one yet (walkable, outside the history, decision 113), and all the animations are set as one undo step; the status says how many ("Animated 13 tiles of waterfall_b."). Re-importing the same file records nothing. `CatalogDraft` (294 lines) first moves its two removals into a partial file, unchanged, so it can grow.

## New types
- `Editor/Catalogs/CatalogDraft.Removals.cs`: the partial file holding `RemoveTilesheet` and `RemoveObject`, moved as they are (about 30 lines; `CatalogDraftRemovalTests` pass unchanged).
- `Editor/Tiles/AnimationImport` (`internal sealed partial class : ObservableObject`): Tiles > Import animations: a `.tsx` matched to a set-up sheet, its animations set as one undo step, or why not.
  - `AnimationImport(CatalogDraft catalog)`.
  - `void Import(string tsxPath)` (reads through `TiledTilesetReader.Read`; a `ContentValidationException` becomes the status, as `SheetImport` reports its refusals); `[ObservableProperty] string Status` (private set).

## Changes
- `Editor/Catalogs/CatalogDraft`: `public void SetTileAnimations(IReadOnlyDictionary<int, TileAnimation?> animations)`: one step setting each tile's animation (null makes it still), reverting each to what it was; nothing when every tile already has what it would get (records compare by value: `TileAnimation` holds no list). The Animate panel reuses it (AE2).
- `Editor/Shell/MainWindow.xaml`: Tiles > Import animations..., and the import's status in the status band beside the image import's.
- `Editor/Shell/MainWindow.xaml.cs` or `MainWindow.TileAnimation.cs` (created here if the handler needs a home; AE2 adds the tick to it): `OnImportAnimationsClicked` shows the `OpenFileDialog` and calls `Import` (view only).
- `Editor/Shell/MainViewModel`: `public AnimationImport? AnimationImport { get; }` (+3 lines).

## Tests
- `Catalogs/CatalogDraftAnimationTests` (new):
  - `SetTileAnimations_TwoTiles_IsOneUndoStep` (one Undo makes both still).
  - `SetTileAnimations_SameAnimations_RecordsNothing`.
  - `SetTileAnimations_Null_MakesTheTileStill`.
- `Tiles/AnimationImportTests` (new; `.tsx` text written by the test, its image named after a test world sheet's file):
  - `Import_MatchingSheet_AnimatesEachCellAsOneStep`.
  - `Import_CellsNotTilesYet_MakesThemTiles`.
  - `Import_NoSheetWithThatImage_ChangesNothingAndSaysSo`.
  - `Import_OtherTileSize_ChangesNothingAndSaysSo`.
  - `Import_AnimationOfAnotherShape_ChangesNothingAndNamesTheTile`.

Size: about 30 lines moved, 55 lines of code and XAML, 8 tests (about 205).

## Verify by hand
- Set up the Forest Autumn animation sheets in the editor (at 16 px, if not yet), then Tiles > Import animations... and choose the pack's waterfall B `.tsx` (outside the repository): the status says how many tiles were animated. Save; `content/tiles.json` shows `animation` on those tiles (rows 0-3: 8 frames at 70 ms; rows 4-9: 4 frames at 140 ms, the developer's count). Do the same with the sparkles `.tsx` (3 tiles of 4 frames at 600 ms). Ctrl+Z undoes one import whole; importing again records nothing new.
- Paint a waterfall tile on an overlay layer over a cliff and the sparkles over water, save, run the game: they animate (AN2). The editor's map shows their first frame (decision 466).
- Choose a `.tsx` whose image isn't set up: the status says no sheet has that image.

- **Done, with deviations:** a `.tsx` with no animated tile is refused with its own status line ("water.tsx: no tile is animated."); two tests more, `Import_SameFileTwice_RecordsOneStep` and `Import_NoAnimatedTile_ChangesNothingAndSaysSo`; the menu entry enables with the content (`CanStartNewObject`); `MainWindow.TileAnimation.cs` is created here for the dialog handler. `CatalogDraft` is at 298 lines after the move and `SetTileAnimations`. From the review: an image set up as several sheets is matched to the one with the `.tsx`'s cut (`Import_ImageSetUpTwice_AnimatesTheSheetWithTheTsxCut`); "1 tile" in the status; AI1's unused `TiledTileset.Columns` left.
