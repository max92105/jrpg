# CP1. Copies of placed objects with fresh ids

Builds on design.md, "What exists" (placed objects) and "Key decisions" 8; decision 512; Risks (pasted ids against cell events).

## Goal
Headless: the rule that turns copied placed objects into new ones for a map. The copies keep their layout relative to each other, their top-left corner of the group at a given point; each gets a uid no object on the target map has (`{objectId}_{n}`, the next free n, as placing does); each event gets `{newUid}_{n}`, keeping its n, or the next free id for one of another shape; no event id clashes with any on the target map, cell events included. Everything else is kept: object id, layer, events' gates, prompts, steps (warps, swaps, dialogue). Placing them is one undo step. Nothing is reachable from the window yet (CP2).

## New types
- `Editor/Maps/Editing/PlacedObjectCopies` (`internal static class`): copies of placed objects for a map: fresh uids and event ids, moved to a new top-left.
  - `public static IReadOnlyList<PlacedObject> Make(IReadOnlyList<PlacedObject> copied, MapLayout target, Point2D topLeft)`: in the copied order; `topLeft` is where the group's bounds' top-left lands (the smallest `Position.X` and `Position.Y` of the copied).
  - `public static Point2D TopLeftOf(IReadOnlyList<PlacedObject> objects)` (the group's corner, read by CP2 to center a paste).
  - Event ids: `{oldUid}_{n}` → `{newUid}_{n}` while that id is free (n read with `NumberedIds.HighestNumber($"{oldUid}_", [id])`, the same digits-only rule); a taken one, or any id of another shape, → `EventIds.Next(newUid, used)`, where `used` holds every event id on the target map (objects' and cells') and those already made.
- `Editor/Maps/Editing/PlaceObjectsEdit` (`internal sealed record PlaceObjectsEdit(IReadOnlyList<PlacedObject> Placed) : IMapEdit`): places several objects on top of the others as one step; revert removes them from the end (undo runs in reverse order, as `PlaceObjectEdit`'s comment says).

## Changes
None outside the new types.

## Tests (new `Maps/Editing/PlacedObjectCopiesTests`, records built in code)
- `PlacedObjectCopiesTests: Make_OneObject_GetsTheNextUidOfItsObject` (target holds `chest_1`, `chest_3`: the copy is `chest_4`).
- `PlacedObjectCopiesTests: Make_TwoCopiesOfOneObject_GetTwoDifferentUids` (`chest_4`, `chest_5`).
- `PlacedObjectCopiesTests: Make_EventsWithTheUidPattern_KeepTheirNumbersUnderTheNewUid` (`chest_1_0`, `chest_1_2` → `chest_4_0`, `chest_4_2`).
- `PlacedObjectCopiesTests: Make_EventWithAnotherId_GetsTheNextFreeId` (`"talk"` → `npc_2_0`).
- `PlacedObjectCopiesTests: Make_CellEventHoldingTheNewId_SkipsIt` (a cell event `chest_4_0` on the target: the copied `chest_1_0` becomes `chest_4_1`, the next free id).
- `PlacedObjectCopiesTests: Make_Group_KeepsTheOffsetsFromItsTopLeft` (two objects at (2.5, 3) and (5, 1), pasted at (10, 10): (10, 12) and (12.5, 10)).
- `PlacedObjectCopiesTests: Make_KeepsObjectIdLayerAndEventContent` (z, gates, prompt, `IsOnce`, a `WarpStep` to another map and a `SwapObjectStep`, equal but for the ids).
- `PlacedObjectCopiesTests: Make_OntoTheMapItCameFrom_LeavesTheOriginalsUnchanged` (the originals' uids and event ids are taken, so none is reused).
- `PlacedObjectCopiesTests: TopLeftOf_Group_IsTheSmallestXAndY`.
- `ObjectEditingTests: PlaceObjectsEdit_ApplyThenRevert_LeavesTheMapAsBefore` (two objects; after revert the draft's objects equal the original list).

Size: about 60 lines of code; 10 tests (about 120): about 180.

## Map
`docs/map.md`, Editor `Maps / Maps/Editing` row: `PlacedObjectCopies` (copies of placed objects with fresh uids and event ids), `PlaceObjectsEdit`.

## Hand check (yours)
Nothing to see yet (CP2).

- **Done:** as planned. `Make_KeepsObjectIdLayerAndEventContent` checks the fields one by one (records holding lists don't compare by value). Review: the summary says a cut original pasted back may take its old uid, as a move would; a test that compared unchanged records was cut down to what it checks. CP2 must not paste an empty clipboard (`TopLeftOf` reads a minimum).
