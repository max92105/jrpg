# L2a. The Shop's frame and list column

Part of [the gold and shops plan](../README.md). Decisions 252, 269 (a page's dark column), 370. References: `docs/ui-reference/shop-sell.webp` (header with the shop name top left and the gold top right, the list with NAME / OWNED / PRICE column titles, footer hint, the map dimmed behind), `menu-equipment.webp`. After L1. The forge plan's F1d is the model. Design: [design.md](../design.md), "Tunables" (fade).

- **L2a. The Shop's frame and list column: header with gold, tabs, column titles, footer, fade** (Game, raw 125, about 200 lines at 1.6x: adds a view type)
  - **Goal:** the Shop in the game's look: the map dimmed behind it; a header bar with the shop's name at top left and the party's gold at top right; Buy and Sell as two pointed tabs above the list (`Ui/PointedBarShape`, as the victory's Items step; the shown one bright); the list in a page column with column titles (Item, Owned, Price); a footer with its hint. It fades in and out through `Ui/OverlayFade` (the Forge's times), never through black.
  - **New types:**
    - `Game/Trade/ShopListView`: draws the list column: the page column, the tabs, the column titles, the shown tab's rows, the gliding selection, the empty line.
  - **Changes:**
    - `Trade/ShopView`: the scrim, header (name, gold measured when it changes), footer, and `ShopListView` at its place (below); the opacity from the scene.
    - `Trade/ShopScene`: an `OverlayFade`; Cancel starts the fade out, and `ShopClosed` comes once it has run, once.
    - Text `shop_hint` (footer: "Left/Right: Buy or Sell · Confirm: trade · Cancel: leave"), `shop_column_item`, `shop_column_owned`, `shop_column_price`; `MessageIds`.
  - **Pixel arithmetic** (1920 x 1080, the menu's frame as the Forge reads it: header 88 (`MenuHeader.Height`), body from 128, footer 72 from 1008, the body 848 tall to 976 (`MenuLayout.BodyHeight`)):
    - List column, x 40 to 1000: tabs 48 + gap 12; column titles 36; 13 visible rows x 52 (the Inventory's row) = 676. Total 772 ≤ 848: 76 px spare; more rows scroll (`ListCursor`'s window). Columns: name from 96 (after a 40 px icon), owned right-aligned at 760, price right-aligned at 960 (the widest, "9999999 G" at the body size, fits 180 px; measured in the slice).
    - Detail column, x 1040 to 1880: L2b.
  - **Tests:**
    - `ShopSceneTests: Update_Cancel_RequestsCloseOnceTheFadeHasRun` (replaces `Update_Cancel_RequestsCloseOnce`)
    - Existing: `GameFlowShopTests` steps through the closing fade (its updates change, not its assertion).
    - Draw-only: before and after captures (F12) instead of invented tests.
  - **Watch:** one selection look and the menu's bands; ask before departing from both the reference and the menu. The gold in the header follows a purchase (T1) without remeasuring every frame.
  - **Verify by hand:** at the shopkeeper, capture with F12 and compare with `shop-sell.webp`: the dimmed map, the header with the name and gold, the tabs, the columns, the footer; the Shop fades in and out over the map.
