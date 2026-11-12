# FS4. The interaction prompt clears a tall NPC's head

Game, about 140 lines raw (60 of them tests, 30 re-pinned). Design: "Replacement mapping". Builds on FS3.

## Goal
The "Talk" prompt floats above a tall NPC's drawn head, not inside it. One rule says where an object is drawn on screen; the map view and the prompt read it.

## New types
- `Game/Exploration/ObjectScreenArea` (internal static): an object's drawn rectangle on screen: its cells, or a field sprite's frame standing on its cell (`FieldSpriteSheet.AreaOver`). `Of(PlacedObject placed, ObjectDefinition definition, IReadOnlyDictionary<string, FieldSpriteSheet> sprites, Vector2 cameraOrigin)`.

## Changes
- `Game/Exploration/MapView`: `ScreenAreaOf` and FS3's NPC arithmetic go; it reads `ObjectScreenArea.Of` (canopies keep the object's cells: a field-sprite object has no zones).
- `Game/Exploration/PromptPlacement.BoxFor(Rectangle objectArea, Point boxSize)`: centered over the area, `GapAboveObject` above its top.
- `Game/Exploration/InteractionPromptView`: its constructor takes the sprites beside `currentDefinitionOf` (three parameters); `Draw` asks `ObjectScreenArea.Of` then `BoxFor`.
- `Game/Flow/SceneFactory.CreateExploration` and `tests/Jrpg.Game.Tests/Exploration/StartMapScenarios` (the two `new InteractionPromptView` sites): pass `catalog.FieldSprites`.

## Tests
- `ObjectScreenAreaTests` (new): `Of_TileObject_CoversItsCells`; `Of_ObjectBetweenCells_RoundsTopLeftOnce` (the 2.5 position case `MapView` handled); `Of_PlayerShapedNpc_IsItsCell`; `Of_TallNpc_RisesAboveItsCell`.
- `PromptPlacementTests` (re-pinned to the rectangle): `BoxFor_ObjectArea_CentersAboveItsTop`; `BoxFor_TallNpcArea_SitsAboveTheHead`.

## Verify by hand
1. With FS3's hand-written bought NPC: face it; the prompt sits above its head with the usual gap, centered.
2. Face a chest and a door: the prompt sits where it did before.
