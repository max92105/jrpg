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

## From the battle plan (2026-10-20)

- **The clamped time share `Math.Clamp((float)(elapsed / length), 0, 1)` is copied 6 times** (`ActionPlayback`, `FloatingNumber` twice, `LevelUpPop`, `VictoryEntrance`, `DefeatEntrance`), and the cubic ease-out twice (`LevelUpPop`, `VictoryEntrance`). One small shared `Ui` timing helper.
- **The footer bar with its hairline is drawn 3 times** (`GameMenuScene`, `VictoryScreenView`, `DefeatScreenView`; the two outcome screens also share the left shade and the title hairline). A shared `Ui` footer, and an outcome frame for the two battle screens.
- **`Ui/MessageIds.cs` is 305 lines** of constants. Split it by feature (menu, battle, title) or group the battle ids.
- **More invariant number formatting inline** (`ToString(CultureInfo.InvariantCulture)` in `ActionPlayback`, `CommandPanelView`, `VictoryScreenView`, `VitalsText`), on top of the copies listed above.
- **Victory screen at the maximum level** shows "EXP 0/0" and an empty bar; hide the EXP row instead (B7a, B7b1 reviews).
- **Victory cards have no row limit**: more than 6 active members reach the footer line (B7b1 review).
- **Enemy attack strips are drawn unmirrored over the party**; no flip rule is decided (B5d). A design question for the developer.
- **"Load the latest save" and the save-list overlay are written twice** (`TitleScene`, `DefeatScreen`/`DefeatScreenView`), and the banner band twice (`BattleBannerView`, `BattleScene.DrawEscaped`). Share each on a third copy.
- `ContentWriter.WriteAll`: the final moves aren't atomic across files, and the cleanup's `File.Delete` can hide the first error when a folder is missing (review of E3c).
