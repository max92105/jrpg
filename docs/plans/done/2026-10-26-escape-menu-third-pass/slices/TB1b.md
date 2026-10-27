# TB1b. The icon rail's look

Part of [the escape menu third pass plan](../README.md). Decision 286. Design: [design.md](../design.md), "Tunables and placeholders" (tab folding), "Risks" (two motions at once). After TB1a. Look at `docs/ui-reference/menu-inventory-description.webp` (the folded tabs: one icon per tab, the selected tab lit, the point toward the page) first, and TB1a's capture.

- **TB1b. The icon rail's look: icons centered in the rail, labels gone by halfway, the selected tab on the rail** (Game, about 120 lines)
  - **Goal:** folded, the tabs read as the reference's icon rail: each icon centered in the visible part of its tab, the labels already gone by the fold's middle so no half-word shows at the screen's edge, the selected tab lit with its icon in place; folding and the page's own slide-in read as one movement.
  - **New types:** none.
  - **Changes (Game):**
    - `Menus/TabFold`: `float LabelOpacity`: 1 to 0 over the first half of the fold, 0 to 1 over the second half of the unfold.
    - `Menus/MenuTabsView`: labels at `LabelOpacity`; each icon's place eases from its unfolded place to the middle between the screen's edge and the tab's shoulder; the selected icon's 14 px slide shrinks to 0 as the tabs fold; the selected tab's glow and line stay on its visible part.
    - `Menus/PageTransition` or `GameMenuScene`: if the page's slide-in from the right fights the area's move to the left in the hand check, the page's slide is shortened (a named constant) and the change noted in the Done note.
  - **Tests:** `TabFoldTests`:
    - `LabelOpacity_HalfwayThroughFolding_IsZero`;
    - `LabelOpacity_QuarterIntoUnfolding_IsStillZero`;
    - `LabelOpacity_AfterUnfolding_IsOne`.
  - **Verify by hand:** folded on every page: icons centered on the rail, none clipped; no label fragment at the edge mid-fold; the selected tab lit with its icon; entering a page reads as one leftward settle. F12 against `menu-inventory-description.webp`.
- **Done, with deviations:** most of the slice landed in TB1a: the folded icons already sit midway along the rail's flat part, the selected icon's 14 px slide already shrinks to 0 (the icon lerps to a folded place with no slide), and the selected tab's glow and line already move with the fold. What this slice adds is `TabFold.LabelOpacity`, kept as its own value rather than computed from `Share`: fading out while folding and back in over an unfold's second half can't be one function of the fold's position without a jump when the player turns back mid-way. So it falls at twice the fold's speed while folding, and rises at that speed once an unfold's share is under 0.125 (where a full unfold is halfway; derived from the fade speed, so retuning one moves the other). `PageTransition` is unchanged: whether the page's slide-in fights the fold is the developer's hand check, since captures can't show motion. Tests: 6 in `TabFoldTests`: the slice's three, `Unfold_EarlyInAFold_KeepsTheLabelsFade`, and from the review `LabelOpacity_ThreeQuartersIntoUnfolding_IsHalfBack` (frame by frame, so it pins the fade speed) and `Fold_LateInAnUnfold_KeepsTheLabelsFade`; 2213 in the solution.
