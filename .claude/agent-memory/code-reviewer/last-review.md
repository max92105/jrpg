---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 2c (Layers and draw order)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

Accepted (don't re-raise): DrawOrderKey/MapLayers have no production caller until 3a/3b/11b; tiles compare base Y too (always 0, tie); comparison operators for CA1036; Z required, builders use Z 0,1,2 and MapLayers.PlayerZ; MapFolderReaderTests JSON gained "z": 0.

- open (Suggestion): MapConversion.cs class doc says canopy baking is "plan 3a"; baking (ObjectSpriteBaker) is slice 3b
- open (Suggestion): DrawOrderKey.ForTile `order` doc says "position among layers with the same Z", but the validator makes layer Zs unique; say "the layer's index in the map"
- open (Suggestion): DrawOrderKeyTests.CompareTo_TilesSameZ_IgnoreBaseAndKeepInsertionOrder: tiles have no base to ignore; rename to CompareTo_TilesSameZ_KeepInsertionOrder
