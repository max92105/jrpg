# DO1. Moves only: one draw order in Core

Core + Game + Editor, about 150 lines changed and 140 lines of tests moved, no behavior change. Builds on design.md, "Core changes" and "Key decisions" 3. Decision 520.

## Goal
The game's map, the editor's map and the editor's hit test draw and pick from one rule in Core. Nothing changes on screen; the existing tests are the guard (`MapDrawListTests`, `MapDrawingTests`, `DrawOrderKeyTests`, the `PlacedObjectHits` tests).

## New types
- `Core/Maps/MapDrawOrder` (public static): the map's parts in the order every drawer draws them.
  - `public const double PlayerFeetBelowPosition = 0.5;` (moved from `MapDrawList`, with its comment).
  - `public static void Fill(List<MapDrawItem> into, MapLayout layout, Func<PlacedObject, ObjectDefinition> definitionOf)`: clears `into`, adds every layer, object and canopy, sorts.
  - `public static void FillWithPlayer(List<MapDrawItem> into, MapLayout layout, Func<PlacedObject, ObjectDefinition> definitionOf, Point2D playerPosition)`: the same with the player after every object in insertion order (today's rule and comment).
  - `public static DrawOrderKey SpriteKeyOf(PlacedObject placed, ObjectDefinition definition, int order)`: a placed object's body key.
  - Sorting uses one static comparer (no allocation per call); the summary says the game calls it every update.
- `Core/Maps/MapDrawItem` (public readonly record struct): one part of the map to draw: `DrawOrderKey Key`, `MapDrawItemKind Kind`, `int Index` (the layer's index or the object's), `int Z` (the layer the part belongs to: today the canopy's own layer, as `MapPart.Z` is).
- `Core/Maps/MapDrawItemKind` (public enum): `Layer`, `Object`, `Canopy`, `Player`.

## Changes
- `Game/Exploration/MapDrawList`: keeps its list and `Rebuild(layout, currentDefinitionOf, playerPosition)`, which calls `MapDrawOrder.FillWithPlayer`; `Drawables` becomes `IReadOnlyList<MapDrawItem>`. `Drawable.cs` and `DrawableKind.cs` are deleted; `ExplorationScene.DrawMap` switches on `MapDrawItemKind`.
- `Editor/Maps/MapDrawing`: `PartsInDrawOrder` calls `MapDrawOrder.Fill` with `catalog.Current.Objects[placed.ObjectId]`; `MapPart` and `MapPartKind` are deleted (check whether they are nested types or files and delete accordingly); the dimming reads `item.Z`.
- `Editor/Maps/Editing/PlacedObjectHits.TopAt`: its key from `MapDrawOrder.SpriteKeyOf`.
- `docs/map.md`: the Maps row of Core adds `MapDrawOrder`/`MapDrawItem`; the Game Exploration row drops `MapDrawList`'s "sort" wording if any; ADR 0012's "Watch" line is closed by ADR 0041 later (no ADR edit here).

## Tests
- Moved from `Game.Tests/Exploration/MapDrawListTests` to a new `Core.Tests/Maps/MapDrawOrderTests`, renamed `Rebuild_` to `FillWithPlayer_` (same bodies over `MapDrawOrder`, counted before and after: 9 tests): `FillWithPlayer_SpriteLowerOnScreen_DrawnLater`, `FillWithPlayer_PlayerBetweenObjectsOnItsLayer_SortsByFeet`, `FillWithPlayer_PlayerFeetOnObjectBase_PlayerDrawnInFront`, `FillWithPlayer_LayersAndObjects_FollowZ`, `FillWithPlayer_CanopyOfObjectBelowPlayer_DrawnAfterPlayer`, `FillWithPlayer_ObjectOnCanopyLayer_HasNoCanopy`, `FillWithPlayer_ObjectWithoutZones_HasNoCanopy`, `FillWithPlayer_Twice_KeepsOneEntryPerThing`, `FillWithPlayer_SwappedObject_UsesCurrentDefinitionHeight`.
- `MapDrawListTests` keeps one test: `Rebuild_TwoMaps_DrawablesFollowTheLastMap` (the game's wrapper reuses its list), or none if the moved `Twice` test already covers it (say which in the Done note).
- New in `MapDrawOrderTests`: `Fill_NoPlayer_HasNoPlayerItem`; `SpriteKeyOf_LowerBase_SortsBefore` (objects set up in the opposite order).
- Existing `MapDrawingTests` and the hits tests pass unchanged.

- **Done, with deviations:** `MapDrawItemKind.Object` is `ObjectBody` (CA1720 refuses a member named `Object`). `MapDrawListTests` keeps `Rebuild_TwoMaps_DrawablesFollowTheLastMap` over layers alone; the nine moved tests and the two new ones are in `MapDrawOrderTests`. `MapDrawOrder.BaseOf` holds the one bottom-edge formula the body and the canopy keys share. From the review: the sort takes a `Comparison` delegate, since `List.Sort(IComparer)` allocated one per update (the old game code did too, 64 bytes measured); `MapDrawList.Drawables` is `Items`.

## Verify by hand
Before/after screenshots of one repository map in the game (F12 capture) and the editor (same map, Fit): identical. Toggle View > Dim other layers in the editor: the same parts dim as before.
