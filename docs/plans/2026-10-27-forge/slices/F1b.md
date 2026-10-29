# F1b. The Forge lists the owned pieces

Part of [the forge plan](../README.md). Decisions 369 (owned pieces only), 362 (rarity colors), 252 (one selection look). After N1 (checked by hand at the forge master). Was the list half of the draft's F1. Reference: `docs/ui-reference/shop-sell.webp` (an NPC-opened screen: a list with a price column), `menu-equipment.webp` (a list beside a detail column). Design: [design.md](../design.md), "Key decisions" (a row index is a gear index).

- **F1b. The Forge lists the owned pieces** (Game + text, about 165 lines; extends `ForgeView`, adds no view type)
  - **Goal:** the Forge lists every owned piece (`Inventory.Gear`, in pickup order; worn ones aren't owned, decision 369), one row each: icon and name in its rarity's color. Up and Down move the shared selection (gliding highlight, bobbing pointer); with no piece, a line says there is nothing to work on and there is no cursor.
  - **New type:**
    - `Game/Crafting/ForgeList`: the Forge's list of owned pieces, its scrolling cursor and gliding highlight, built when the items change (like `Menus/InventoryList`). `ForgeList(Inventory inventory)`; `IReadOnlyList<GearPiece> Pieces`; `ListCursor? Cursor` (null when empty); `SelectionMotion Selection`; `GearPiece? SelectedPiece`; `Show(int selectedRow)` (the cursor to that row, or the last once the list got shorter; the highlight jumps there); `MoveUp()`, `MoveDown()`, `Advance(TimeSpan delta)`.
  - **Changes:**
    - `Crafting/ForgeScene`: takes the session; Up and Down move the list; `Advance` passes the delta to the list.
    - `Crafting/ForgeView`: the rows (icon, name in `UiTheme.RarityColorOf`), the selection through `UiTextures.DrawGlidingSelection` and `MenuPointer`, the empty line. Plain placement (the look is F1d).
    - `Flow/SceneFactory.CreateForge(GameSession session)`, `Flow/ForgeTransitions.Open(GameSession session)`, `GameFlow` passes the session.
    - Text `forge_empty` ("Nothing to work on."); `MessageIds.ForgeEmpty`.
  - **Tests:**
    - `ForgeListTests: Show_OwnedGear_ListsEveryPieceInPickupOrder` (a session with stacks too: only the pieces show)
    - `ForgeListTests: Show_NoPieces_HasNoCursor`
    - `ForgeListTests: MoveDown_HighlightsTheNextPiece`
    - `ForgeSceneTests: Update_Down_MovesTheList` (the scene's routing)
  - **Call sites:** `CreateForge()` (1, `ForgeTransitions`), `ForgeTransitions.Open()` (1, `GameFlow`), `new ForgeScene(` in `ForgeSceneTests` (2).
  - **Verify by hand:** at the forge master: the owned pieces listed, a Magic piece in blue; a worn piece absent (leave, take one off on the Equipment page, talk again: it appears); Down and Up glide the highlight.
