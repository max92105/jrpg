# OL2. The stack drawn bottom first

Builds on design.md, "What exists" (both shells draw the list in order) and "Key decisions" 3; decisions 474 and 476.

## Goal
A stacked object looks right everywhere. The game's `MapView` and the editor's `MapDrawing` already draw an object's tiles in list order, which OL1 made the bottom-first order: they change by a comment only, and the editor's object picture gets a test that pins it. The game's canopy bake, which overwrote every pixel, now lays each tile over the pixels under it, so a window's clear pixels show the wall under it in the canopy too. (`SheetPurge`, `CatalogUsage` and `PlacedObjectHits` ask "any tile" of an object and need nothing.)

## New types
None.

## Changes
- `Game/Rendering/ObjectSpriteBaker.CopyTile`: writes `top + under × (1 − top.A / 255)` per channel (premultiplied "over", with the same comment as `UiTheme.Over`; `Rendering` doesn't reference `Ui`). The class summary's "every object draws on one layer: an object tile has no Z of its own" becomes: an object's tiles are listed bottom level first (ADR 0040) and each is laid over the pixels under it; an object still draws on one map layer.
- `Game/Exploration/MapView.DrawObject`: a comment on the loop: the list is bottom level first (ADR 0040), so drawing it in order draws the stack.
- `Editor/Maps/MapDrawing.DrawObjectAt`: the same comment.

## Tests
- `ObjectSpriteBakerTests: CanopyPixels_TransparentPixelOnTheUpperLevel_KeepsTheBaseTilesPixel` (a two-tile catalog on one cell: the base red, the upper tile with one transparent pixel).
- `ObjectSpriteBakerTests: CanopyPixels_OpaquePixelOnTheUpperLevel_CoversTheBaseTile`.
- `ObjectSpriteBakerTests: CanopyPixels_HalfTransparentPixelOnTheUpperLevel_BlendsOverTheBaseTile` (a pinned premultiplied value, e.g. half-alpha white over opaque red).
- `MapDrawingTests: DrawObject_StackedObject_DrawsTheUpperLevelLast` (the second `ImageDrawing` is the upper tile's image; the preview, the ghost and the Shapes window all use `DrawObject`).
- Guards that must pass unchanged: `ObjectSpriteBakerTests`' two existing tests (the first tile over a transparent canopy is today's copy).

Size: about 20 lines of code, 4 tests (about 85): about 105.

## Map
No new type, nothing to change in `docs/map.md`.

## Hand check (yours)
None here: the repository has no zoned object to look at, and nothing makes a stacked one before the composer can. OL4's hand check, steps 9 to 15, draws an above-player zone over a stacked object and walks behind it in the game.
