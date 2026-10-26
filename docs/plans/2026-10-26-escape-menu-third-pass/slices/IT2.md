# IT2. The pointer stays on the sidebar while the item has copies left

Part of [the escape menu third pass plan](../README.md). Decision 288. Design: [design.md](../design.md), "Tunables and placeholders" (aiming and preview: after a use). After IT1.

- **IT2. The pointer stays on the sidebar while the item has copies left** (Game, about 140 lines)
  - **Goal:** after a use, if the item still has copies and someone it can go to, the pointer stays on the sidebar, on the same member if they can still take it, else on the first who can; the result shows under the list's row and the list's quantity drops. When the stack runs out, or nobody can take the item any more, the pointer returns to the list. Cancel returns to the list at any time.
  - **New types:** none.
  - **Changes (Game):**
    - `Menus/ItemUse`: after `UseOn`, if `FieldItemUse.CanUse(item)` still holds, a new picker over the new target choice, pointing at the same member when they're still a candidate (`ItemTargetPicker.PointAt(PartyMember)`, a no-op for a non-candidate); otherwise aiming ends. Its result line is returned as today.
    - `Menus/ItemTargetPicker`: `PointAt(PartyMember member)`.
    - `Menus/InventoryPage`: after a use, rebuilds its rows (keeping the list's selected row) whether or not aiming continues; the hint stays the target hint while aiming.
  - **Tests:**
    - `InventoryPageAimTests`:
      - `Update_UsePotionWithCopiesLeft_KeepsAimingAtTheSameMember`;
      - `Update_UseLastPotion_ReturnsToTheList`;
      - `Update_ReviveTheOnlyFallenWithCopiesLeft_ReturnsToTheList` (nobody left to aim at);
      - `Update_ReviveOneOfTwoFallen_AimsAtTheOtherFallen`;
      - `Update_CancelAfterAUse_ReturnsToTheListAndUsesNothingMore`.
    - `ItemTargetPickerTests`: `PointAt_Candidate_PointsAtIt`, `PointAt_NonCandidate_KeepsThePointer`.
    - Check each existing use test in `InventoryPageTests` and `InventoryPageRestItemTests`: one that asserts `IsChoosingTarget` is false after a use with copies and a target left changes with decision 288; name it in the Done note. `Update_UseLastElixir_…` and the take-all revive (nobody fallen after it) keep passing.
  - **Verify by hand:** two hurt members and three potions: heal one, the pointer stays on them, heal again, move to the other, heal; the third potion gone, the pointer is back on the list. A revive with one fallen: after it, back to the list. F12 mid-aim after a use (the result under the row, the quantity down).
