# Cleanup backlog

Code-health work waiting for the next cleanup plan; `/retro` appends to it. One line per item, with where it came from. A cleanup plan (`/plan-feature`) starts from this list and removes the lines it covers.

## From the menu cleanup plan (2026-10-20)

- **`InventoryPage` is 326 lines and `StatusPage` 289**, neither split yet. Split them like `EquipmentPage` and `PartyPage`: a pure part, views, and a page that keeps input.
- **The private `Number(int)` invariant formatter is copied about 6 times** (`EquipmentSheet`, `StatPreviewText`, `StatusPage`, `SaveSlotListView`, `MemberCardView`, the count in `PartyColumnsView`). One shared `Ui` formatter would replace them.
- **Text stored next to its width by hand** in `InventoryPage.ItemRow`, `InventoryLabels`, `SaveSlotListView`, `ConfirmPromptView`, `MenuHeader` and `EquipmentChoiceView`. Use `Ui/MeasuredText`.
- **Column titles over a hairline are copied 3 times** (`InventoryPage.DrawColumnTitles`, `EquipmentChoiceView`, `Saving/SaveSlotListView`). A shared `Ui/ColumnTitles`.
- **`StatusPage`'s own slot labels** could use `EquipmentSheet.Slots` and `EquipmentSheet.LabelIdOf`.
- **Why Core refused an equipment change**: `EquipmentPage.Apply` works out "no room in the bag" itself, and "Remove all" loops over the slots in the Game. A Core result should own both.
- **Item target picker layout** (question 112, placeholder kept): with an odd member count the last card sits alone in the left column, and 14 or more members (8 rows) don't fit 1080 px.
