# Cleanup backlog

Code-health work waiting for the next cleanup plan; `/retro` appends to it. One line per item, with where it came from. A cleanup plan (`/plan-feature`) starts from this list and removes the lines it covers.

The code health plan (`done/2026-10-23-code-health`) did the lines it covered; the ones its review moved wait under "When next touched" below.

## When next touched

These waited at the 2026-10-23 code health review. Do each one with the next change to its file, or when a plan needs it. Each says why it can wait.

- **A fixed class resource that leaves out `startsAt` loads and starts at 0.** `ResourceDefinition`'s `Max` and `StartsAt` default to 0 so a speed-scaled resource can leave them out (RG1), and 0 is a valid start, so the validator can't tell the two apart. Make both nullable when a content edit trips on it. (class-mechanics RG1 review)
- **`TextureLibrary` keeps every texture it loads for good**, cutscene stills included. That is fine with the 4 KB placeholder stills, but full-screen real stills (about 8 MB of video memory each, 3 per intro) would stay loaded after the intro. Drop a still once its event ends when real art arrives. (new-game-flow K2a)
- **An open yes/no question is held and drawn the same way in 3 screens** (`SavePage`, `SystemPage`, `NewGame/LeaderChoiceScene`: a `ConfirmPrompt?` field, a `ConfirmPromptView`, drawn while open, cleared when answered). Each copy is a few lines, and what Yes does differs per screen. Pull it into a small shared `Ui` type when a fourth screen needs it. (new-game-flow S1b review)
- **A dialog a menu page draws leaves the footer hint bright**: `GameMenuScene.Draw` draws the footer after the page, so the scrim of the Party notice and of the Save and System prompts dims everything but the hint. Draw page dialogs after the footer, or the footer under the page, when the walk-through or the next menu change gets to it. (escape-menu-third-pass PD1 review)
- `MainViewModel` (489 lines after E3): the sheet image-size cache (`SheetImageSizes`) and the import status text (`SheetImportResult.Summary`) could leave it.
- **`Directory.Build.props`:** merge `src/` and `tests/` into a root file. Decision 144 keeps two files for now; the developer didn't choose to merge. When it happens, use the standard form:
  - the gates go in a root `Directory.Build.props`;
  - `tests/Directory.Build.props` keeps its two test-only settings (`IsPackable=false`, `using Xunit`) and imports the root file through `$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))`;
  - no path condition.

  The file is protected, so ask the developer first.
- **Why Core refused an equipment change:** `SlotChoice.Apply` works out "no room in the bag" itself, and "Remove all" loops over the slots in the Game. `PartyEquipment.ItemWithoutRoom` and `UnequipAll` would own both. It can wait because it is correct today, and nothing planned changes equipment.
- **`Ui/MessageIds.cs` is 410 lines** of constants. Split it into partial files by feature, so no call site changes. It can wait because a table of constants doesn't get harder to change as it grows.
- **`MainViewModelTests`, part 2 (449 lines after E1a):** move the new content tests (`CreateObject_*`, `CreateNpc_*`, `CanStartNewObject_*`, `TryCreateMap_*`) and the map settings tests (`TryApplySettings_*`) into their own classes, on E1a's `EditorTestContent`.
- **`MainWindow.xaml.cs` (547 lines):** move the viewport (pan, zoom, fit) and the tile painting handlers into partial files, as `.EventEditing` and `.ObjectEditing` already are.
- **`TileBrowserViewModelTests` (410 lines):** move the Cut panel tests (Set up, Cut, Apply, Cancel, with `OpenGrass3Cut` and `Ask`) into a `TileBrowserCutTests` class.
- **`TileBrowserViewModel` (about 300 lines):** move the sheet hit-testing (`CellAt`, `NearestCellTo`, `GridPositionOf`, F1's grid check) into a `Tiles/SheetGrid`.
- **`ObjectShapesViewModelTests` (347 lines):** move the zone tests (E10) and the point-editing tests (E11a) into their own classes.
- **The New NPC dialog** doesn't check that the chosen sprite holds the walk frames. The startup and save check (plan 2026-10-23, F5a) refuses such a sprite, and `NewNpcViewModel.SpriteProblem` could say so in the dialog, through a size lookup.

## From the battle plan (2026-10-20)

- **Enemy attack strips are drawn unmirrored over the party**; no flip rule is decided (B5d). A design question for the developer.
- **"Load the latest save" and the save-list overlay are written twice** (`TitleScene`, `DefeatScreen`/`DefeatScreenView`). Share them in the **autosave plan**, which is likely to add the third copy.
- **The banner band** is drawn only by `BattleScene.DrawEscaped` now (the action name moved into a framed window in W1a). Share it with the **map name banner plan** when that plan draws the second one.
- `TileImages.Forget` keeps cached field-sprite frames; drop them too once field sprites can be imported (no import yet).

## From the demo retirement plan (2026-10-23)

- Editor: with a button focused (Fit, Add layer), Space + drag pans, but releasing Space still clicks the button (WPF clicks a focused button on Space up; Space is swallowed only while the map has focus). Older than F2, easier to meet since F2 (review of F2).

## From the new-game flow plan (2026-10-24)

- The editor's map save (`ContentWriter.WriteMap`) doesn't check that the files a map's events name exist: still images (`ArtReferenceCheck`) and voice files (`AudioReferenceCheck`). The game's startup rejects both, so an editor save can write content the game won't start with (`editor.md`, "Validate on save").
- `Core/Exploration/ExplorationState.cs` (296) is near 300. The next feature touching it should split it first: starting events (arrival, cell, touch) apart from walking.
- Test files over 300 lines: `GameFlowTests` (369), `PartyStateTests` (322). Split by behavior when next touched.
- Comment lines over 120 columns from before the rule was enforced: `ExplorationState` (5), `GameSession` (8), `ArtReferenceCheck` (2), `MessageIds` (1). Rewrap when next touched.
- `ScreenFade` skips its black moment when one update is longer than its fade-out plus hold (about 0.33 s), so a warp never moves the player. The fixed 1/60 s step hides it in play; tests must step at 1/60 s. Make `Update` stop on Holding if a long frame can ever reach it.
- `BattleState.cs` is at 296 lines: split the per-command bodies before the next command path lands (DR2 review).
- `tests/Jrpg.Game.Tests/Battle/CommandMenuTests.cs` is at 393 lines: split it by menu entry (MO1 review).
- The tip strip (dark `UiTheme.CommandHintFill` fill, highlight hairline, padded text) is drawn twice: `Battle/CommandPanelView.DrawHint` and `Menus/InventoryListView.DrawStrip`. Move it into one drawer in `Ui` when a third appears (M4 review).
- The gap under a section title's band, `SectionTitle.Height + 10`, is written three times (`EquipmentSheetView.SectionHeight`, `StatusSheetView.BelowBand`, `StatusSkillsView.BelowBand`): give `SectionTitle` a `ContentTop` (M3b review; the band's height itself moved there in the escape menu third pass, F1).
- The scroll cues above and below a list are drawn twice: `PartySidebarView.DrawScrollCues` and `MemberColumnView.DrawScrollMarks`. Move them into one `Ui` drawer if a third list needs them (M2b review).

## From the class mechanics plan (2026-10-25)

- `tests/Jrpg.Game.Tests/Exploration/EventPresenterTests.cs` (304): split by behavior when next touched.
- Core can't say whether a turn was kept: the screen keeps the actor forward both after a quick skill and when the same member gets two turns in a row (a readied turn right before their own; LK2b review). A `TurnKept` event from `TurnFlow.KeepTurn` would tell them apart.
- `UiTheme.ColorFromHex("")` throws, and the Core test fixtures build resources without a color; a Game test that reaches them through a battle panel would fail (LK1a review). Give the test builders a color, or make the validator's rule a Core default.
- Placeholder marks waiting on icon art: Move borrows the Run icon (FM2), and the extra turn's hourglass is a bright diamond (LK3).

## From the UI second pass plan (2026-10-25)

- `content/art/backgrounds/GameMenu.png` is 4096 x 2838 (23 MB, about 46 MB of video memory) for a 1920 x 1080 screen: a 1920 x 1330 copy would decode about five times faster (BG1). The developer's art; ask first.
- The battle's skill hint doesn't wrap: a long description runs past its box over the actor's portrait (`docs/captures/before/battle-skill.jpg`; SZ2).
- **Centering a line of text in a row is written about 10 times**, as `row.Y + ((RowHeight - font.LineHeight) / 2f)`: `EquipmentChoiceView`, `EquipmentSheetView` (twice), `InventoryListView`, `OptionsPage`, `SystemPage`, `PartyActionChoiceView` (integer division), `ConfirmPromptView` and others. A small `Ui` helper could hold it, as `FooterBar.TextTop` does for footers. It can wait because each copy is one line. (C2 review)
- `src/Jrpg.Game/Battle/CommandPanelView.cs` is at 281 lines: split it before the next change that grows it (retro on the other machine).
- No debug way to start on a given map and cell: Z's automated walk (on the other machine) couldn't reach the inn or the forest, so those hand checks fell back to the developer. A Debug-only `--start <map> <x> <y>` beside `--content` would let `play-game.ps1` reach any screen.

## From the escape menu third pass plan (2026-10-26)

- Two refusals are still red lines while the Party page's open the OK dialog (`Ui/NoticeDialog`, decision 268): Equipment's notice under its parameters (`EquipmentSheetView`, `UiTheme.Danger`) and the save page's failure line (`SavePage`). Kept as they are by decision 284; move them to `NoticeDialog` if the developer asks for it.

## From the escape menu third pass plan (2026-10-27)

- `Menus/PartyPage.cs` (275) is near 300: the next change that grows it splits the notice and the action choice into their own presenter first. (`InventoryPage` split its list into `InventoryList` in the loot plan, C1.)
- A titled dialog's frame (the scrim, a `FramedWindowLayout.Titled` window, the centered title, measured when the dialog changes) is written twice: `ConfirmPromptView` and `NoticeDialogView`. Share it when a third dialog comes, such as Equipment's or the save page's notice (decision 284).
- The inventory hint names the Info button as "X (Y on a controller)" in `ui.json`, a copy of `InputBindings`; give the hint a `{button}` from `ButtonLabels.Of(GameButton.Info, device)` as `cinematic_skip` does, once menu pages know the device in use (F1 review).
- Status: the Skills section starts under the longer of the gear and resource rows, so the stats plan's nine slots will push it off the page; that plan places it (F1 review).
- Lists outside the menu still draw `MenuPointer` at the cursor's row, against the glide rule (`.claude/rules/game.md`, decision 336): the battle's command list (`CommandPanelView`), the defeat menu (`DefeatScreenView`) and the leader choice (`LeaderChoiceScene`). Glide them when next touched; the battle's look follows later anyway (decision 272).

## From the stats and gear slots plan (2026-10-27)

- `Menus/EquipmentPage.cs` (281) is near 300: the next change that grows it splits the choice handling (opening, picking, the notice) into its own presenter first.
- `tests/Jrpg.Data.Tests/Saving/FileSaveStoreTests.cs` (289) and `SaveGameRepairTests.cs` (285): split by concern (the envelope and format checks apart from the round trips; members apart from inventory and world) when next touched.
- `Battle/ActionPlayback.Read` is at the complexity limit (the H1 and I1 slices each had to fold cases to pass CA1502): the next `BattleEvent` kind needs a handler table, not another case.
- `SaveGameRepair.RepairMember` sums worn bonuses from item ids beside `WornEquipment.Bonuses` (G1b): a public `WornEquipment` built from the catalog would let the repair use the one sum.
- `ClassCatalogValidator.FindUnreachableRequirementProblem` checks a skill's requirement against the resource's definition max, not its max under Will (`raiseMax`); conservative today, wrong once a skill needs Faith above 5 (W1b review).
- Labels are uppercased in three places (`StatusLabels.Caps`, `EquipmentLabels.From`, `EquipmentChangesView.Measure`): `SheetRowLook.DrawLabel` could cap once, if measuring stays out of `Draw` (E2 review).
