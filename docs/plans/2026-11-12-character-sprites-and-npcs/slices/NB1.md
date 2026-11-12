# NB1. An NPC faces the player while its own event runs

Core + Game, about 190 lines raw (110 of them tests). Design: "Core changes", key decision 4. Decisions 571, 577. Builds on FS3. ADR 0044 written first.

## Goal
Talk to an NPC from its left: it turns to face the player (right) for the whole conversation, the shop or the Forge it opens included, and turns back down when the event ends. A cell event or another object's event leaves it as it stands. Nothing is saved.

## New types
- `Core/Geometry/Facings` (public static): `Opposite(Facing)` (throws on an unknown value).
- `Core/Exploration/NpcFacing` (internal static): the facing an NPC shows: `Of(PlacedObject placed, PlacedObject? speaking, Facing playerFacing)`: the player's opposite when `speaking` has the placed object's uid, else `Facing.Down` (NB2 replaces the fallback with the placed facing).

## Changes
- `Core/Exploration/EventRun`: `Source` (the placed object the event belongs to, null for a cell event), read from the `TriggeredEvent` it holds.
- `Core/Exploration/ExplorationState` (247): `NpcFacingOf(PlacedObject placed)` (`NpcFacing.Of(placed, _events.Run?.Source, PlayerFacing)`), public as the Game reads it.
- `Game/Exploration/MapView`: its constructor takes `Func<PlacedObject, Facing> npcFacingOf` (four parameters: layout, current definition, art, facing); an NPC draws `StandingCell(npcFacingOf(placed))`.
- `Game/Exploration/ExplorationScene`: builds the delegate once in its constructor (`_npcFacingOf = exploration.NpcFacingOf`, no allocation per frame, as `_currentDefinitionOf`) and passes it in `EnterCurrentMap`; `MapViewTests`' constructions follow.

## Tests
- `FacingsTests` (new): `Opposite_EachFacing_IsTheOtherWay` (theory); `Opposite_UnknownValue_Throws`.
- `NpcFacingTests` (new): `Of_NoEventRunning_FacesDown`; `Of_OwnEventRunning_FacesThePlayer` (theory over the four player facings); `Of_OtherObjectsEventRunning_FacesDown`; `Of_SameUidAfterSwap_FacesThePlayer` (the speaking object compared by uid).
- `ExplorationNpcFacingTests` (new, Core `Exploration/`, through `TestExplorationMap`): `NpcFacingOf_TalkedToFromItsLeft_FacesLeftThenDownAfter` (interact, read the step, continue to the end); `NpcFacingOf_CellEventRunning_StaysDown`; `NpcFacingOf_ShopOpenedByItsEvent_FacesThePlayerUntilClosed`.

## Verify by hand
1. Start the game, walk to the left of an NPC, face it, Confirm: it turns toward the player while its lines show; after the last line it faces down again.
2. Talk to it from above, below and the right: it faces the player each time.
3. A shopkeeper NPC: it faces the player while the Shop is open and turns back after the Shop closes.
