# DO2. Canopies keep their object's layer

Core + Editor, about 150 lines (a throwaway test of about 40 lines written, run and deleted inside the slice). Builds on DO1, ADR 0041 and design.md, "Diagnosis: canopies flatten the layers" and "Key decisions" 4. Decisions 521, 522 (Q3).

## Goal
An object on a higher layer draws over an object on a lower layer, canopies included: a tree on -3 overlapping a house on -4 draws its leaves over the house's roof even when the house's bottom edge is lower on screen. On one layer nothing changes. What stays (decision 521): leaves are always over the player and walls always under, so a tree behind a house (on a lower layer) still shows its leaves over the house's walls where they overlap, as today.

## Order inside the slice
1. Write `MapDrawOrderTests.Fill_TreeOnMinus3HouseOnMinus4HouseLower_DrawsTheLeavesOverTheRoof` and see it fail (today both canopies sit on layer 5 sorted by base, so the roof comes last). Note it in the Done note.
2. Write the throwaway `RepositoryCanopyFlipsTests` (below), run it on today's rule and on the new one, paste its output into the Done note, then **delete it in the same slice** (say so in the Done note).
3. The fix, then the other tests.

## Changes
- `Core/Maps/DrawOrderKey`:
  - a private `_ownerZ`: the layer the part comes from; `ForTile` and `ForSprite` set it to their `z`.
  - `public static DrawOrderKey ForCanopy(int objectZ, double baseY, int order) => new(MapLayers.CanopyZOf(objectZ), SpriteGroup, objectZ, baseY, order);`
  - `CompareTo`: Z, then group, then owner, then base, then order.
  - Summary rewrapped: "on one Z, map tiles before sprites; sprites by the layer they come from (a canopy: its object's), then their base...".
- `Core/Maps/MapDrawOrder`: canopy items use `ForCanopy`; a canopy item's `Z` is its object's Z (decision 522), so the editor's dimming treats the canopy with its object. The "canopyZ > placed.Z" test keeps deciding whether a canopy exists.
- `Editor/Maps/MapDrawing`: nothing beyond what `item.Z` now says; reread its summary for "canopy layer" wording.

## Tests
`Core.Tests/Maps/MapDrawOrderTests` (each set up with the expected-later object listed first):
- `Fill_TreeOnMinus3HouseOnMinus4HouseLower_DrawsTheLeavesOverTheRoof`: a house on -4 with a roof zone, its base at 8; a tree on -3 with a leaves zone, its base at 7, overlapping: the whole order asserted (layers, house body, tree body, house canopy, tree canopy, by kind and index).
- `Fill_TreeOnMinus5InFrontOfHouseOnMinus4_LeavesUnderTheRoof`: the tree's base lower on screen than the house's: the tree's canopy before the house's.
- `Fill_SameLayerCanopies_LowerBaseDrawsLater` (today's rule kept).
- `Fill_TreeBehindHouseOnLowerLayer_LeavesStillAfterTheHousesBody` (the remaining limit pinned: the tree's canopy comes after the house's body).
- `Fill_CanopyItem_TakesItsObjectsLayer`: the canopy item's `Z` is the object's.
`Core.Tests/Maps/DrawOrderKeyTests`:
- `CompareTo_SameZSpritesFromDifferentLayers_HigherOwnerDrawsLater` (the higher-owner key built with the lower base).
- `CompareTo_TileAndCanopyOnOneZ_TileFirst` (the group still wins over the owner).
`Editor.Tests/Maps/MapDrawingTests`: `Draw_FocusOnObjectsLayer_CanopyNotDimmed` if the file can observe the dimming through its existing seam; otherwise the Done note says the Core test covers `Z`.

Throwaway, deleted in this slice: `Core.Tests` or `Data.Tests` `RepositoryCanopyFlipsTests.ListFlippedPairs` (in `Data.Tests`, which loads the repository's `content` through the source tree's content root, `AssemblyMetadata ContentRoot`): for each repository map, each pair of placed objects that both have above-player zones, on different layers, whose rectangles overlap, and whose canopy order differs between the old key (base only) and `ForCanopy`; it writes "map, uid A (z, base), uid B (z, base), overlap cells" through `ITestOutputHelper` and always passes. Reads layouts and the object catalog only, never an image (decision 438).

## Verify by hand
1. For each pair in the Done note's flip list: open the map, find the two objects at the listed cells, compare with a capture taken before the slice (F12 in the game, or the editor at 100 %): the higher layer's leaves now draw over the lower layer's roof.
2. On the start map (houses on -4 and -5, trees on -3 and -4): Objects tab, Layer box "-3" (type it in the z box), place a tree with its leaves over the roof of a house on -4 whose bottom edge is lower than the tree's: the leaves draw over the roof (before: under it).
3. Layer -5: place a tree in front of that house (its base lower on screen), leaves over the roof: the roof draws over the leaves (before: the leaves over the roof).
4. A tree on -5 behind the house: its leaves still show over the house's walls where they overlap (unchanged, decision 521's limit).
5. Two trees on -3 overlapping each other: unchanged, sorted by their bases. Walk the player under all of them in the game (F5): leaves over the player, trunks and walls under, as before.
6. View > Dim other layers with layer -4 chosen: the house's roof stays bright with its walls.
