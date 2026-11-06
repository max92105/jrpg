# WP1. The warp destination picker's rules

Builds on design.md, "Key decisions" 6, "Warp destinations: the alternatives" and Risks (the warp warning is advice); decisions 506 and 507; C7. Needs MS1 (`Maps/MapEntry`).

## Goal
Headless: the picker's state. It lists the maps a warp can lead to; it starts on a given map and cell (an unknown or empty map: no map; a cell outside the map: no cell); a cell is chosen inside the chosen map (outside is ignored); choosing another map keeps the cell when it fits, else clears it. A cell is warned about, not refused, when the player can't stand there (no tile, a blocked tile, inside an object's collision outline) or arriving would start an event there (a walk-into cell event, or a walk-into event of an object covering the cell). OK is refused while no map or no cell is chosen. Nothing uses it yet (WP2, WP3).

## New types
- `Editor/Events/WarpMaps` (`internal sealed record WarpMaps(IReadOnlyList<MapEntry> Entries, IReadOnlyDictionary<string, MapLayout> Layouts, ContentCatalog Catalog)`): the maps a warp can lead to: their rows and layouts, and the catalog saying where the player stands. `public static WarpMaps Empty { get; }` (the dialogs built without maps).
- `Editor/Events/WarpDestinationViewModel` (`internal sealed partial class : ObservableObject`): the destination picker's map and cell, its warning and its problem.
  - `WarpDestinationViewModel(WarpMaps maps, string mapId, GridCell? cell)`.
  - `IReadOnlyList<MapEntry> Maps`; `[ObservableProperty] string? MapId` (raises `Layout`; keeps or clears `Cell`; raises `Warning`, `Problem`); `MapLayout? Layout`; `GridCell? Cell` (private set); `string CellText` ("Cell (3, 4)", or "Click a cell on the map").
  - `void ChooseCell(GridCell cell)`: ignored outside the layout.
  - `string? Warning`, the first that applies:
    - "The player can't stand on (3, 4): no tile, or a blocked tile." (`WalkabilityGrid.Build(layout, catalog.Tiles).IsWalkable`);
    - "(3, 4) is inside the collision outline of chest_2." (each placed object's `CollisionShapes.OutlineOf`, below, with `Polygon.Contains` at the cell's center);
    - "Arriving on (3, 4) starts event door_1_0 (walked into)." (a cell event with `EventActivation.Touch` on the cell, or a `Touch` event of an object whose bounds hold the cell's center).
    Null otherwise.
  - `string? Problem`: "Choose the map to warp to." / "Click the cell to arrive on."; null when both are chosen.
  - `CellZoom Zoom { get; } = new(32)` (the picture's cell size, WP3; tunables).

## Changes
- `Maps/CollisionShapes`: `public static IReadOnlyList<Point2D> OutlineOf(PlacedObject placed, ObjectDefinition definition)` (its private `InMapCells` applied to one object's outline, now shared), so the picker tests each placed object's outline knowing its uid, and no outline is worked out twice.

## Tests (new `Events/WarpDestinationViewModelTests`, the test world's catalog, layouts built in code: a 4 x 3 map of walkable tiles with one blocked tile, an object with an outline, a walk-into cell event and an object with a walk-into event; a 2 x 2 map)
- `New_MapAndCell_StartsOnThem`; `New_UnknownMap_StartsWithNoMapAndAProblem`; `New_CellOutsideTheMap_StartsWithNoCell`.
- `ChooseCell_InsideTheMap_SetsItAndItsText`.
- `ChooseCell_PastTheRightEdge_LeavesTheCell`; `ChooseCell_AtMinusOne_LeavesTheCell` (one test per side).
- `MapId_ChangedToAMapTheCellFits_KeepsTheCell`; `MapId_ChangedToASmallerMap_ClearsTheCell`.
- `Warning_BlockedCell_NamesIt`; `Warning_CellWithNoTile_NamesIt`.
- `Warning_CellInsideAnOutline_NamesTheObject`; `Warning_CellBesideTheOutline_IsNull` (the accepted side).
- `Warning_CellWithAWalkIntoCellEvent_NamesTheEvent`; `Warning_CellUnderAnObjectWithAWalkIntoEvent_NamesTheEvent`; `Warning_CellWithAPressedEventOnly_IsNull` (an interact event doesn't start on arrival).
- `Warning_WalkableEmptyCell_IsNull`.
- `Problem_NoCell_AsksForTheCell`; `Problem_MapAndCell_IsNull`.
- `ChooseCell_RaisesCellTextWarningAndProblem`.
- `CollisionShapesTests: OutlineOf_PlacedObject_IsItsOutlineInMapCells`.

Size: about 95 lines of code (the view model 80, the record 8, `OutlineOf` 5); 21 tests (about 140, one shared fixture): about 235. Its tests are one-assert each over one fixture.

## Map
`docs/map.md`, Editor `Events` row: `WarpMaps`, `WarpDestinationViewModel` (the warp step's destination picker: map, cell, a warning on a cell the player can't stand on or that starts an event).

## Hand check (yours)
Nothing to see yet (WP3).

- **Done, with deviations:** the warning is its own static `WarpArrivalWarning`, so the view model holds only the choice. `WarpMaps.Empty` and the picker's `Zoom` wait for their users (WP2, WP3). An object's walk-into event counts only when it runs on that object (`CanRunOn`), with a test. Review: the game starts nothing on arrival, so the walk-into warnings say when the event starts (a cell's on stepping off and back on, an object's on the first step), decision 507's wording; a test for the notices on a map change; the fixture builds its own tiles.
