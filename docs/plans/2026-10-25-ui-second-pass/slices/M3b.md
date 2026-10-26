# M3b. Status page look

Part of [the UI second pass plan](../README.md). Decisions 252, 261. Design: [design.md](../design.md), "Tunables and placeholders" (status labels). After M3a. Look at `docs/ui-reference/menu-status.webp`, `menu-field-skills.webp` (the sidebar's "HP 2649/2649" and "LV 35": small labels, large numbers) and the index's "Status" section first.

- **M3b. Status page look: spaced-caps labels beside large numbers, hairline rows** (Game, about 170 lines)
  - **Goal:**
    - Each parameter row reads like the reference: the label small, in spaced caps and secondary color (`HP`, `ATTACK`, `DEFENSE`, `SPEED`, `LV`, `NEXT`), the value large and right-aligned in its column, a 1 px hairline under the row; skills and resources take the same rows.
    - "LV" plus a large number beside the name; the experience bar keeps its place.
  - **Changes (Game):**
    - `Menus/StatusSheet`: caps labels built with the sheet (`ToUpperInvariant` once), value widths measured then (`MeasuredText`), so drawing allocates nothing.
    - `Menus/StatusSheetView`: label and value fonts (SZ1's sizes), letter spacing 2 for labels (`DrawText(..., characterSpacing:)`), the hairline in `UiTheme.Stroke`.
  - **Tests** (`StatusPageTests`): `Open_FirstMember_LabelsAreInCaps` (through `ShownLabels`, like `ShownStats`).
  - **Verify by hand:** compare with `menu-status.webp`: small caps labels, large numbers on their right edge, hairlines between rows, nothing overlapping for the longest class, skill and item names; numbers change when equipment changes.
- **Done, with deviations:**
  - **The labels keep their messages, in caps:** "LEVEL", "EXP", "NEXT LEVEL", "HP", "ATTACK", "DEFENSE", "SPEED", the slots and the resource names, rather than new "LV" and "NEXT" texts, since `status_level` is also the leader choice's label. `StatusLabels` upper-cases its captions once and `StatusSheet` the resource names.
  - **The values are measured by the view** (`StatusSheetView.Show`, in the 30 px value font), not by `StatusSheet`, which stays free of fonts; the view keeps the sheet and its measured values together (`ShownSheet`).
  - **One row look** (`DrawRow`): the 18 px label spaced 2 px, its foot on the value's line; the 30 px value on the column's right edge; a 1 px `UiTheme.Stroke` hairline under it. The parameters, what's worn and the resources use it; each skill gets the hairline under its description. Worn item names are in the value font too.
  - **The level sits on the name's line**, at the profile's right edge; the experience section keeps EXP and NEXT LEVEL over the bar.
  - **Tests:** `Open_FirstMember_LabelsAreInCaps` (2157 in all); the resource tests now expect the names in caps. `StatusSheetView` is 254 lines.
