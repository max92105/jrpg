# Cleanup backlog

Code-health work waiting for the next cleanup plan; `/retro` appends to it. One line per item, with where it came from. A cleanup plan (`/plan-feature`) starts from this list and removes the lines it covers.

The code health plan (`done/2026-10-23-code-health`) did the lines it covered; the ones its review moved wait under "When next touched" below.

## When next touched

These waited at the 2026-10-23 code health review. Do each one with the next change to its file, or when a plan needs it. Each says why it can wait.

- **A fixed class resource that leaves out `startsAt` loads and starts at 0.** `ResourceDefinition`'s `Max` and `StartsAt` default to 0 so a speed-scaled resource can leave them out (RG1), and 0 is a valid start, so the validator can't tell the two apart. Make both nullable when a content edit trips on it. (class-mechanics RG1 review)
- **`TextureLibrary` keeps every texture it loads for good**, cutscene stills included. That is fine with the 4 KB placeholder stills, but full-screen real stills (about 8 MB of video memory each, 3 per intro) would stay loaded after the intro. Drop a still once its event ends when real art arrives. (new-game-flow K2a)
- **An open yes/no question is held and drawn the same way in 3 screens** (`SavePage`, `SystemPage`, `NewGame/LeaderChoiceScene`: a `ConfirmPrompt?` field, a `ConfirmPromptView`, drawn while open, cleared when answered). Each copy is a few lines, and what Yes does differs per screen. Pull it into a small shared `Ui` type when a fourth screen needs it. (new-game-flow S1b review)
- `MainViewModel` (489 lines after E3): the sheet image-size cache (`SheetImageSizes`) and the import status text (`SheetImportResult.Summary`) could leave it.
- **Centering the hint in a footer is written 3 times** (`GameMenuScene.DrawFooterHint`, `VictoryScreenView.Draw`, `DefeatScreenView.DrawFooter`). It could become `FooterBar.TextTop(height, lineHeight)`. The menu rounds to whole pixels and the outcome screens don't, so merging them moves the menu hint by up to a pixel; do it with the next footer change, checked by screenshot. (U6 review)
- **`Directory.Build.props`:** merge `src/` and `tests/` into a root file. Decision 144 keeps two files for now; the developer didn't choose to merge. When it happens, use the standard form:
  - the gates go in a root `Directory.Build.props`;
  - `tests/Directory.Build.props` keeps its two test-only settings (`IsPackable=false`, `using Xunit`) and imports the root file through `$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))`;
  - no path condition.

  The file is protected, so ask the developer first.
- **Why Core refused an equipment change:** `EquipmentPage.Apply` works out "no room in the bag" itself, and "Remove all" loops over the slots in the Game. `PartyEquipment.ItemWithoutRoom` and `UnequipAll` would own both. It can wait because it is correct today, and nothing planned changes equipment.
- **Easing copies:** the clamped time share `Math.Clamp((float)(elapsed / length), 0, 1)` is copied 6 times (`ActionPlayback`, `FloatingNumber` twice, `LevelUpPop`, `VictoryEntrance`, `DefeatEntrance`), and the cubic ease-out twice (`LevelUpPop`, `VictoryEntrance`). They should use `Ui/Easing`. It can wait because each copy is one line.
- **`Ui/MessageIds.cs` is 397 lines** of constants. Split it into partial files by feature, so no call site changes. It can wait because a table of constants doesn't get harder to change as it grows.
- **`MainViewModelTests`, part 2 (449 lines after E1a):** move the new content tests (`CreateObject_*`, `CreateNpc_*`, `CanStartNewObject_*`, `TryCreateMap_*`) and the map settings tests (`TryApplySettings_*`) into their own classes, on E1a's `EditorTestContent`.
- **`MainWindow.xaml.cs` (547 lines):** move the viewport (pan, zoom, fit) and the tile painting handlers into partial files, as `.EventEditing` and `.ObjectEditing` already are.
- **`TileBrowserViewModelTests` (410 lines):** move the Cut panel tests (Set up, Cut, Apply, Cancel, with `OpenGrass3Cut` and `Ask`) into a `TileBrowserCutTests` class.
- **`TileBrowserViewModel` (about 300 lines):** move the sheet hit-testing (`CellAt`, `NearestCellTo`, `GridPositionOf`, F1's grid check) into a `Tiles/SheetGrid`.
- **`ObjectShapesViewModelTests` (347 lines):** move the zone tests (E10) and the point-editing tests (E11a) into their own classes.
- **The New NPC dialog** doesn't check that the chosen sprite holds the walk frames. The startup and save check (plan 2026-10-23, F5a) refuses such a sprite, and `NewNpcViewModel.SpriteProblem` could say so in the dialog, through a size lookup.

## From the menu cleanup plan (2026-10-20)

- **Item target picker layout** (question 112, placeholder kept): with an odd member count the last card sits alone in the left column, and 14 or more members (8 rows) don't fit 1080 px.

## From the battle plan (2026-10-20)

- **Enemy attack strips are drawn unmirrored over the party**; no flip rule is decided (B5d). A design question for the developer.
- **"Load the latest save" and the save-list overlay are written twice** (`TitleScene`, `DefeatScreen`/`DefeatScreenView`). Share them in the **autosave plan**, which is likely to add the third copy.
- **The banner band** is drawn only by `BattleScene.DrawEscaped` now (the action name moved into a framed window in W1a). Share it with the **map name banner plan** when that plan draws the second one.
- `TileImages.Forget` keeps cached field-sprite frames; drop them too once field sprites can be imported (no import yet).

## From the demo retirement plan (2026-10-23)

- Equipment page: opening a slot's choice hides the party sidebar (484 px) but the choice panel is 560 px, so the sheet narrows by 76 px while it is open (P1/Q2). Waits for question 153, the sidebar's placement.
- Editor: with a button focused (Fit, Add layer), Space + drag pans, but releasing Space still clicks the button (WPF clicks a focused button on Space up; Space is swallowed only while the map has focus). Older than F2, easier to meet since F2 (review of F2).

## From the new-game flow plan (2026-10-24)

- The editor's map save (`ContentWriter.WriteMap`) doesn't check that the files a map's events name exist: still images (`ArtReferenceCheck`) and voice files (`AudioReferenceCheck`). The game's startup rejects both, so an editor save can write content the game won't start with (`editor.md`, "Validate on save").
- The footer hint's vertical centering is written out in `GameMenuScene.DrawFooterHint`, `DefeatScreenView.DrawFooter` and `VictoryScreenView`. Move them onto `FooterBar.TextTop`, which the leader choice uses (S3a).
- `Core/Exploration/ExplorationState.cs` (296) is near 300. The next feature touching it should split it first: starting events (arrival, cell, touch) apart from walking.
- Test files over 300 lines: `GameFlowTests` (369), `InventoryPageTests` (325), `PartyStateTests` (322). Split by behavior when next touched.
- Comment lines over 120 columns from before the rule was enforced: `ExplorationState` (5), `GameSession` (8), `ArtReferenceCheck` (2), `MessageIds` (1). Rewrap when next touched.
- `ScreenFade` skips its black moment when one update is longer than its fade-out plus hold (about 0.33 s), so a warp never moves the player. The fixed 1/60 s step hides it in play; tests must step at 1/60 s. Make `Update` stop on Holding if a long frame can ever reach it.
- `BattleState.cs` is at 296 lines: split the per-command bodies before the next command path lands (DR2 review).
- `tests/Jrpg.Game.Tests/Battle/CommandMenuTests.cs` is at 393 lines: split it by menu entry (MO1 review).
- The tip strip (dark `UiTheme.CommandHintFill` fill, highlight hairline, padded text) is drawn twice: `Battle/CommandPanelView.DrawHint` and `Menus/InventoryListView.DrawStrip`. Move it into one drawer in `Ui` when a third appears (M4 review).
- `SectionHeight = 36` (a `SectionTitle`'s height) is copied in `EquipmentSheetView`, `StatusSheetView` and `StatusSkillsView`: expose it from `Ui/SectionTitle` (M3b review).
- The scroll cues above and below a list are drawn twice: `PartySidebarView.DrawScrollCues` and `MemberColumnView.DrawScrollMarks`. Move them into one `Ui` drawer if a third list needs them (M2b review).

## From the class mechanics plan (2026-10-25)

- `tests/Jrpg.Core.Tests/Battle/BattleStateTests.cs` is 361 lines: split it by command.
- `tests/Jrpg.Core.Tests/Party/PartyMemberTests.cs` (307) and `tests/Jrpg.Game.Tests/Exploration/EventPresenterTests.cs` (304): split by behavior when next touched.
- Core can't say whether a turn was kept: the screen keeps the actor forward both after a quick skill and when the same member gets two turns in a row (a readied turn right before their own; LK2b review). A `TurnKept` event from `TurnFlow.KeepTurn` would tell them apart.
- `UiTheme.ColorFromHex("")` throws, and the Core test fixtures build resources without a color; a Game test that reaches them through a battle panel would fail (LK1a review). Give the test builders a color, or make the validator's rule a Core default.
- Placeholder marks waiting on icon art: Move borrows the Run icon (FM2), and the extra turn's hourglass is a bright diamond (LK3).

## From the UI second pass plan (2026-10-25)

- `content/art/backgrounds/GameMenu.png` is 4096 x 2838 (23 MB, about 46 MB of video memory) for a 1920 x 1080 screen: a 1920 x 1330 copy would decode about five times faster (BG1). The developer's art; ask first.
- The battle's skill hint doesn't wrap: a long description runs past its box over the actor's portrait (`docs/captures/before/battle-skill.jpg`; SZ2).
