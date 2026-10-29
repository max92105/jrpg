# L2b. A member's stat preview for gear

Part of [the gold and shops plan](../README.md). Decisions 383 (one member, the leader first, Left and Right switch; reuse the Equipment page's comparison), 337 (what a comparison lists). References: `docs/ui-reference/shop-sell.webp` (the party's changes beside the list), `menu-equipment.webp`. After L2a. Design: [design.md](../design.md), "Key decisions" (the Shop's controls), "Risks" (the detail column's height).

- **L2b. A member's stat preview for gear; Left and Right switch the member** (Game + text, about 200 lines; reuses `EquipmentChangesView`, adds no view type)
  - **Goal:** on a gear row (either tab), the detail column shows one member's name and what wearing that piece would change, as the Equipment page shows it ("Changes": each changed stat now and after, in the up or down color); the leader first. Left and Right in the list switch through the active party (wrapping); the member stays chosen while the cursor moves and when the tab changes. A consumable or material row shows no preview.
  - **New types:** none.
  - **Changes:**
    - `Menus/EquipmentComparison.ShowPiece(PartyMember member, GearPiece piece)`: the member now against wearing the piece in the first slot its kind fits (`EquipmentFit.SlotsFor(kind)[0]`; a ring is compared in the first ring slot). Its summary names the Shop as the second user.
    - `Trade/ShopScene`: the previewed member (an index into the active party) and its `EquipmentComparison`, rebuilt when the row or the member changes; Left and Right in the list.
    - `Trade/ShopDetailView`: the member's name line and `Menus/EquipmentChangesView` under it (caption "Changes").
    - Text: the footer hint names Left and Right on gear; `MessageIds`.
  - **Pixel arithmetic** (L2a's 446 px, then): member name line 40, a 16 px gap, `EquipmentChangesView.Height` (46 band + 10 + 5 lines x 40 = 256). Total 446 + 312 = 758 ≤ 848, 90 px spare. If the measured description needs a fourth line (32), 58 px still spare.
  - **Tests:**
    - `ShopScenePreviewTests: Update_GearRow_PreviewsTheLeader` (new file)
    - `ShopScenePreviewTests: Right_OnGearRow_PreviewsTheNextActiveMember`
    - `ShopScenePreviewTests: Left_OnTheLeader_WrapsToTheLastActiveMember`
    - `ShopScenePreviewTests: Update_ConsumableRow_HasNoPreview` (the case the preview doesn't apply to)
    - `EquipmentComparisonTests: ShowPiece_Ring_ComparesInTheFirstRingSlot` (new file)
  - **Watch:** the comparison is rebuilt only when the row or the member changes, never per frame. A one-member party: Left and Right change nothing.
  - **Verify by hand:** recruit a second member, open the shop: on the rolled sword, the leader's changes; Right: the second member's; on a potion, no preview; Down to the cap: the same member, the cap's changes.
