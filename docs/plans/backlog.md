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
- Editor: Space + drag pans only if the map had focus when Space was pressed (`HandleViewportKey` checks focus before setting `_isSpaceHeld`); with the Tiles panel focused, the drag paints instead (review of E2).
- `MainViewModel` (596 lines after N3; 367 after E4a): move the unsaved-change questions (`ConfirmClose`, `ConfirmLeaveCurrentMap`, which repeat their wording) or the map-list rebuild into their own type.
- `MainViewModel` (596 lines after N3; 467 after E4b2): move the undo routing (journal, `Undo`/`Redo` commands, which mirror each other, and the step event wiring) into its own type.
- `TileBrowserViewModelTests` (345 lines after E6b): move the Cut panel tests (Set up, Cut, Apply, Cancel, with `OpenGrass3Cut` and `Ask`) into a `TileBrowserCutTests` class (review of E6b).
- `MainWindow.xaml.cs` (547 lines after E12c; 512 after E6a): move the catalog and map redraw handlers into their own type (review of E6a).
- Tests: the header-only PNG writer is copied in `ArtReferenceCheckTests` and `ContentWriterTests` (Data); the Editor tests share `PngHeaderFile` (E7b). Share the Data copies on the next change there.
- `TileBrowserViewModel` (about 300 lines after E12a): move the sheet hit-testing (`CellAt`, `NearestCellTo`, `GridPositionOf`) into its own type (review of E12a).
- Re-cutting a sheet to a smaller grid leaves `TileBrowserViewModel.SelectedCell`/`SelectedCells` past the new grid; painting then makes a tile outside the image, which the save refuses. Clear or clamp them on a re-cut, before E12b builds objects from them (review of E12a).
- Editor `TileImages.FieldSpriteFrameOf`: a sprite smaller than a frame is now shown whole (N3), but nothing validates a field sprite's frame layout; `Forget` keeps cached sprite frames (matters once sprites can be imported). From N2's review.

## From the editor plan (2026-10-22 retro)

- `MainViewModelTests` is 770 lines: split it by area (saving, undo journal, new map/object/NPC, layer list) like the view model it tests.
- `ObjectShapesViewModelTests` is 347 lines: move the zone tests (E10) and the point-handle tests (E11a) into their own classes.
- `MainViewModel.CreateNpc` and `CreateObject` end with the same three lines (`SetObject`, choose it in the Objects panel, Objects mode): one private method when `MainViewModel` is split (N3 review).
- Three tests pin repository content totals (283 tiles, the start map's object count, the forest's layers): rewrite them to assert the entries they need, so content edits made in the editor don't break them (E12c; see the retro's tests rule).
