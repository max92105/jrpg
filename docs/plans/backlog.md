# Cleanup backlog

Code-health work waiting for the next cleanup plan; `/retro` appends to it. One line per item, with where it came from. A cleanup plan (`/plan-feature`) starts from this list and removes the lines it covers.

The code health plan (`done/2026-10-23-code-health`) did the lines it covered; the ones its review moved wait under "When next touched" below.

## When next touched

These waited at the 2026-10-23 code health review. Do each one with the next change to its file, or when a plan needs it. Each says why it can wait.

- **A fixed class resource that leaves out `startsAt` loads and starts at 0.** `ResourceDefinition`'s `Max` and `StartsAt` default to 0 so a speed-scaled resource can leave them out (RG1), and 0 is a valid start, so the validator can't tell the two apart. Make both nullable when a content edit trips on it. (class-mechanics RG1 review)
- **`TextureLibrary` keeps every texture it loads for good**, cutscene stills included. That is fine with the 4 KB placeholder stills, but full-screen real stills (about 8 MB of video memory each, 3 per intro) would stay loaded after the intro. Drop a still once its event ends when real art arrives. Real art has arrived: Ragnir's intro holds six 1672 x 941 stills, about 6 MB of video memory each, about 38 MB together (`cinematics/ragnir_intro/`), kept loaded for now (decision 619). (new-game-flow K2a)
- **An open yes/no question is held and drawn the same way in 3 screens** (`SavePage`, `SystemPage`, `NewGame/LeaderChoiceScene`: a `ConfirmPrompt?` field, a `ConfirmPromptView`, drawn while open, cleared when answered). Each copy is a few lines, and what Yes does differs per screen. Pull it into a small shared `Ui` type when a fourth screen needs it. (new-game-flow S1b review)
- **`Directory.Build.props`:** merge `src/` and `tests/` into a root file. Decision 144 keeps two files for now; the developer didn't choose to merge. When it happens, use the standard form:
  - the gates go in a root `Directory.Build.props`;
  - `tests/Directory.Build.props` keeps its two test-only settings (`IsPackable=false`, `using Xunit`) and imports the root file through `$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))`;
  - no path condition.

  The file is protected, so ask the developer first.
- **Why Core refused an equipment change:** `SlotChoice.Apply` works out "no room in the bag" itself, and "Remove all" loops over the slots in the Game. `PartyEquipment.ItemWithoutRoom` and `UnequipAll` would own both. It can wait because it is correct today, and nothing planned changes equipment.
- **`TileBrowserViewModelTests` (496 lines):** split it into Cut and re-cut classes (Set up, Cut, Apply, Cancel, with `OpenGrass3Cut` and `Ask`) over a shared setup when the tile browser is next touched (outside the cleanup plan's scope, decision 536).
- **`TileBrowserViewModel` (332 lines since the sheet zoom):** move the sheet hit-testing (`CellAt`, `NearestCellTo`, `GridPositionOf`, F1's grid check) into a `Tiles/SheetGrid`.
- **`ObjectShapesViewModelTests` (347 lines):** move the zone tests (E10) and the point-editing tests (E11a) into their own classes.

## From the battle plan (2026-10-20)

- **Enemy attack strips are drawn unmirrored over the party**; no flip rule is decided (B5d). A design question for the developer.
- **"Load the latest save" and the save-list overlay are written twice** (`TitleScene`, `DefeatScreen`/`DefeatScreenView`). Share them in the **autosave plan**, which is likely to add the third copy.
- **The banner band** is drawn only by `BattleScene.DrawEscaped` (the action name moved into a framed window in W1a); the map banner (the map fixes plan's MB2) drew a different fading band, so a third banner shares, not the second.
- `TileImages.Forget` keeps cached field-sprite frames; drop them too once field sprites can be imported (no import yet).

## From the demo retirement plan (2026-10-23)

- Editor: with a button focused (Fit, Add layer), Space + drag pans, but releasing Space still clicks the button (WPF clicks a focused button on Space up; Space is swallowed only while the map has focus). Older than F2, easier to meet since F2 (review of F2).

## From the new-game flow plan (2026-10-24)

- The editor's map save (`ContentWriter.WriteMaps`, decision 518) doesn't check that the files a map's events name exist: still images (`ArtReferenceCheck`) and voice files (`AudioReferenceCheck`). The game's startup rejects both, so an editor save can write content the game won't start with (`editor.md`, "Validate on save").
- Test files over 300 lines: Core's `PartyStateTests` (322). Split by behavior when next touched.
- Comment lines over 120 columns from before the rule was enforced: `ExplorationState` (5), `GameSession` (8). Rewrap when next touched.
- `ScreenFade` skips its black moment when one update is longer than its fade-out plus hold (about 0.33 s; the 0.08 s hold alone jumps too, so `EventPresenter.ShowWarp` never sees `Holding` and the warp's fade runs out and in over the old map, then starts again with its sound: the sound pass's FE3). Unreachable in play (decision 557): `JrpgGame` sets `IsFixedTimeStep` with a 1/60 s step, and MonoGame's `Game.Tick` (develop branch) passes `ElapsedGameTime = TargetElapsedTime` to every catch-up `DoUpdate`, its backlog capped at 500 ms; tests step at 1/60 s. Reopen (let a fade report a hold it passed) if the step is ever made variable.
- `BattleState.cs` is at 283 lines after the enemy plan's B2 (296 at DR2): split the per-command bodies before the next command path lands (DR2 review).
- The tip strip (dark `UiTheme.CommandHintFill` fill, highlight hairline, padded text) is drawn twice: `Battle/CommandPanelView.DrawHint` and `Menus/InventoryListView.DrawStrip`. Move it into one drawer in `Ui` when a third appears (M4 review).
- The gap under a section title's band, `SectionTitle.Height + 10`, is written three times (`EquipmentSheetView.SectionHeight`, `StatusParametersView.BelowBand`, `StatusSheetView.AttributesBelowBand`; `StatusSkillsView`'s is 4 since decision 373): give `SectionTitle` a `ContentTop` (M3b review; the band's height itself moved there in the escape menu third pass, F1).
- The scroll cues above and below a list are drawn twice: `PartySidebarView.DrawScrollCues` and `MemberColumnView.DrawScrollMarks`. Move them into one `Ui` drawer if a third list needs them (M2b review).

## From the class mechanics plan (2026-10-25)

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

- The save page's failure line (`SavePage`) is still a red line while the Party page's refusals open the OK dialog (`Ui/NoticeDialog`, decision 268). Kept as it is by decision 284; move it to `NoticeDialog` if the developer asks for it. (Equipment's notice left with the room rule, loot plan G1b-2.)

## From the escape menu third pass plan (2026-10-27)

- `Menus/PartyPage.cs` (296 after the music plan's S4) is at the limit: the next change that grows it splits the notice and the action choice into their own presenter first. (`InventoryPage` split its list into `InventoryList` in the loot plan, C1.)
- A titled dialog's frame (the scrim, a `FramedWindowLayout.Titled` window, the centered title, measured when the dialog changes) is written twice: `ConfirmPromptView` and `NoticeDialogView`. Share it when a third dialog comes, such as Equipment's or the save page's notice (decision 284).
- The inventory hint names the Info button as "X (Y on a controller)" in `ui.json`, a copy of `InputBindings`; give the hint a `{button}` from `ButtonLabels.Of(GameButton.Info, device)` as `cinematic_skip` does, once menu pages know the device in use (F1 review).
- Lists outside the menu still draw `MenuPointer` at the cursor's row, against the glide rule (`.claude/rules/game.md`, decision 336): the battle's command list (`CommandPanelView`), the defeat menu (`DefeatScreenView`) and the leader choice (`LeaderChoiceScene`). Glide them when next touched; the battle's look follows later anyway (decision 272).

## From the stats and gear slots plan (2026-10-27)

- `Battle/ActionPlayback.Read` is at the complexity limit (the H1 and I1 slices each had to fold cases to pass CA1502): the next `BattleEvent` kind needs a handler table, not another case.
- `SaveGameRepair.RepairMember` sums worn bonuses from item ids beside `WornEquipment.Bonuses` (G1b): a public `WornEquipment` built from the catalog would let the repair use the one sum.
- `ClassCatalogValidator.FindUnreachableRequirementProblem` checks a skill's requirement against the resource's definition max, not its max under Will (`raiseMax`); conservative today, wrong once a skill needs Faith above 5 (W1b review).
- Labels are uppercased in three places (`StatusLabels.Caps`, `EquipmentLabels.From`, `EquipmentChangesView.Measure`): `SheetRowLook.DrawLabel` could cap once, if measuring stays out of `Draw` (E2 review).
- A one-stat `StatBonuses` is built inline about 20 times as `new(new Dictionary<StatKind, int> { [stat] = value })` (`GearPiece.BonusOf`, `GearPieceTests.Bonus`, `MemberSheetTests.Bonus`, `PartyMemberGearTests` and others): a `StatBonuses.Of(stat, value)` factory would replace them (loot plan, G1a review).
- Equipment's `maxStack` is no longer read since gear became pieces (loot plan, G1b-1): drop it from equipment entries in `items.json` and the validator's rule for them.
- `WholeChance = 100` is declared in three Data validators (`EnemyCatalogValidator`, `StatusGrantValidator`, `DropTableCatalogValidator`): a percent check in `RangeProblems` would hold it once (loot plan, D2 review).
- `.claude/scripts/reflow-comments.pl` (run by `verify.ps1 -Fix`) sometimes turns the first line of a rewrapped `///` block into `//` or `/// //` (loot plan: S1a's `SaveGameFixtures`, P1b's `VictoryDropRow`, both blocks holding a character outside ASCII or an edited first line): the build catches it as bad XML or a doubled summary, but the script should keep the prefix.
- "A rarity's color unless Common, else a fallback" is written twice (`InventoryListView.TextColorOf`, `EquipmentChoiceView.Show`): a third copy moves it into `UiTheme.Loot` as `RarityColorOr(rarity, fallback)` (loot plan, P1b review).

## From the loot plan (2026-10-29)

- `Battle/VictoryScreenView.cs` is at 281 lines (263 after the Items step, D3; 281 after LV3's learned-skill lines): the next change that grows it moves the cards' drawing into a `VictoryCardsView` first.
- `tests/Jrpg.Core.Tests/Session/GameSessionSaveTests.cs` is at 270 lines: the next save test goes in a new file split by concern (gear, members, world).

## From the affix stats plan (2026-10-29)

- `Battle/ActionPlayback` counts a target's numbers already shown this beat twice (`_numbers.Count(… ShowsAt == TimeSpan.Zero)`, for a hit and for stolen life, A1): a third copy extracts a `StackOf(target)`.
- `Menus/StatusParametersView.Draw` draws the Resistances' and Affixes' section titles inline while `DrawCombat` draws its own (ST1b review): a `DrawSection(frame, title, section, cells)` would keep `Draw` at one level; it also has a doubled blank line above `Place`.

## From the forge plan (2026-10-29)

- The "lists 'x' more than once" check is written in six Data validators (`NewGameValidator`, `ClassCatalogValidator`, `ResourceDefinitionValidator`, `SkillCatalogValidator`, `MapEventValidator`, `CraftingRulesValidator`), and `NewGame`'s `startingItems` repeats the crafting lists' entry checks (null, known item, 1..maxStack): one helper beside `RangeProblems` (M4 review).
- `Ui/OverlayFade` (the Forge's and the Shop's fade, gold plan L1) is the one frame fade for screens over the map; `MenuMotion` (the tabs' slide too) and `TitleEntrance` (from black) stay separate because they do more than this fade; fold them in only if a third screen needs the extra.
- `ForgeScene.Craft` re-derives Core's "the first equal piece is the one replaced" rule to put the highlight on the changed row (F2c): `Inventory.TryReplace` (or the Forge's `Try` methods) could return the replaced index instead.

## From the quests plan (2026-10-29, at approval)

- **A page list's margins (28, 20) and row height (52) are private constants in about eight views**: `Crafting/ForgeListView`, `Menus/InventoryColumnView` and `InventoryListView`, `EquipmentChoiceView`, `EquipmentSheetView`, `PartyColumnsView`, `StatusSheetView`, `Saving/SaveFilesPanel` (and the gold plan's `Trade/ShopListView`). Once the quests plan's L1 has made `Ui/PageListLayout`, each reads it when next touched. It can wait because the copies hold the same numbers today.
- **The pointed tabs are drawn inline twice**: `Battle/VictoryDropsView` (the Items step) and the gold plan's `Trade/ShopListView` (Buy and Sell). Once the quests plan's L2 has made `Ui/PointedTabsView`, both draw through it when next touched. It can wait because each copy is drawn once per screen and looks the same.
- `ShopStockRepair` never caps a saved stock's `RolledAtLevel` by the experience table's top level (S6 review): after content lowers the max level below it, that shop never rerolls; bring it down with a report line (decision 360's rule).
- `ForgeView` and `ShopView` draw the same frame (scrim, header, footer, the two column areas from `MenuLayout`), the second copy (gold plan H1): share it in one type once `MenuLayout`'s frame numbers live in `Ui/` (Ui can't read `Menus`).
- `ForgeList` and `Menus/InventoryList` each hold the cursor-plus-gliding-highlight pair that `Ui/RowSelection` now packages for the Shop's lists (gold plan B2): switch them to it when next touched.
- `StatusSkillsView.DescriptionLayout` (a wrapped-text layout) has two users, the Status page and the Shop's detail column (gold plan L2a); move it to `Ui/` and fix its doc, which still names the Status page's font size.
- `ShopListView.Draw` and `ForgeListView.Draw` take 5 parameters (the limit is 4): pass the fixed area to their constructors, as `ShopDetailView` does (L2b review).
- `ShopScene` is at 288 lines after the sound pass's SH3 (the preview left in SH1): the next slice that adds to it splits the list's input (moves, category changes and their sounds) into its own type first.

## From the gold and shops plan (2026-11-01)

- `Game/Exploration/EventPresenter` is at 284 lines after the cinematics plan (CG0 moved its cutscene part to `CutscenePresenter`; CG2 to CR3 added the cinematic's seams) (its page reading left for `MessageReading` in the sound pass's FE1); the next growth past about 300 lines moves the rest's handling (`ShowRest` and the rest's line in `StartReading`) beside `RestPlayback` first.
- The "move down until the wanted row" test helper is written three times (`ShopSceneBuyTests`, `ShopSceneSellTests`, `ShopScenePreviewTests`): `Trade/ShopDriver.MoveDownUntil` (the sound pass, SH2) is the shared one they can switch to.
- Private draw helpers over four parameters: `ShopDetailView.DrawRow` (6), `DrawPreview` and `DrawPicker` (5), `ShopListView.DrawTabs`, `DrawRowSelection` and `DrawRow` (5); fold the area or the opacity into a per-frame record when each view is next touched.
- The gold plan's out-of-scope features (editing shops in the editor, gold in the save list, a `giveGold` step, Unique items in shops, the whole party's preview at once) are listed in `docs/plans/done/2026-10-29-gold-and-shops/README.md`, "Out of scope".
- `CopiedContentRoot` copies every file of a content root (the music included); `ContentCatalogLoaderShopTests` and `ContentCatalogLoaderCraftingTests` keep their own loop on purpose (they copy only the repository's `*.json`), and `EditorTestContent` keeps its own (Editor.Tests can't reach Data.Tests' internal fixture); the two writer test classes moved onto it in the cleanup plan's D2.
- `ObjectIds.ProblemOf` is the content id rule for any typed id (the Quests window uses it for quests and stages, QE2); rename it to a `Catalogs/ContentIds` shared by objects and quests.

## From the quests plan (2026-11-01, at the retro)

- `Core/Session/GameSession.cs` is at 289 lines (the quests plan added the journal and the log's capture and restore): the next member moves the save capture and restore into their own type first.
- `tests/Jrpg.Data.Tests/Content/ContentWriterTests.cs` (254 after the cleanup plan's D2): new tests go in sibling files, as `ContentWriterQuestTests` and `EventPresenterQuestTests` do.
- `Objects/ObjectIds.ProblemOf`'s message says "(like tree_4)" and the Quests window shows it for quest and stage ids (QE2): reword it with the `ContentIds` rename above.
- `Ui/ResourceGaugeView.LabelSpacing` (2) duplicates `SheetRowLook.LabelSpacing`, which `QuestDetailView` reads too (L3 review): one constant in `Ui/`.
- The quests plan's out-of-scope features (the Quest giver preset, removing a saved quest or stage in the Quests window, a cell's touch event that changes with a quest, rewards, story flags, map markers) are listed in `docs/plans/done/2026-10-29-quests/README.md`, "Out of scope".

## From the enemy skills and bosses plan (2026-11-01)

- `BossFightSimulation.PlayFight` repeats the battle setup and loop of `ForestFightsSimulation.RunBattle`: share them when a third simulation needs them (S1 review).
- `Battle/ActionPlayback` is at 292 lines after the music plan's B2; `Read` kept its case count only by sending `EnemyPhaseEntered` through its default arm into `AddCue` (B3): the next event kind brings the handler table the `ActionPlayback.Read` line asks for.
- `BattleTurnSequenceTests` writes `new EnemyTactics(random, new Dictionary<string, SkillDefinition>())` five times (T1): a small helper, as `EnemyTacticsTests` has.
- The enemy plan's out-of-scope features (a battle event step and a boss beaten for good, more conditions, enemy resources, cooldowns, phases that change stats or art, a boss banner, skills for the goblin and the beetle) are listed in `docs/plans/done/2026-10-29-enemy-skills-and-bosses/README.md`, "Out of scope".

## From the music and sound effects plan (2026-11-01)

- `SystemPageTests`' older tests (`Update_ConfirmQuit_AsksBeforeRequesting`, `Update_YesToQuit_RequestsQuit`, `Update_YesToTitleScreen_RequestsTitle`, `Update_AnswerNo_StaysInPage`) and the new `Update_ConfirmQuit_AsksNothing` reach rows by fixed presses: `SystemPage` exposes no selected entry (S3 review).
- `BattleSceneTests` and `BattleSceneSoundTests` reach command rows by fixed presses (Up wrapping to Run, Down counted to Defend), since `BattleScene` doesn't expose its menu's selection (B4 review).
- `Battle/BattleScene` is at 290 lines after the sound pass's V1: the next change that grows it moves its outcome screens' update and finish into their own type first.
- `InventoryPageSoundTests` and the older Inventory tests lean on the list opening on the potion: a small helper that moves to an item by id (S4 review).
- The music plan's out-of-scope features (settings kept between runs, composed music, crossfades, streaming, field sounds, a `playMusic` step, pausing on lost focus) are listed in `docs/plans/done/2026-10-29-music-and-sfx/README.md`, "Out of scope".

## From the Mana Seed reset plan (2026-11-03, at approval)

- The plan's out-of-scope features (removing a sheet's unused tiles while the sheet stays, decision 450; an autotile or Wang brush; animated tiles; Mana Seed characters and monsters against `FieldSpriteLayout`'s 48 px frames; the lighting layer, decision 445; map removal in the editor; deleting an image from the editor; Import writing the art manifest entry, decision 448; a release check refusing placeholder art; undoing the purge) are listed in `docs/plans/done/2026-11-03-mana-seed-reset/README.md`, "Out of scope".
- `SaveGameRepair` checks only that a saved map still exists (decision 186): a save on a rebuilt map with the same id loads at its old position, maybe in a wall or past a shrunk edge. Repair a position outside the map or on a blocked cell to the new game's start, or the map's arrival, when saves outlive a map rebuild (after the first release).
- `SoundSourcesTests` reads the test output's copy of `content/`, which keeps a deleted file until a clean build (`PreserveNewest` never deletes): read the repository's folder through the `ContentRoot` metadata the Mana Seed plan's AS1 stamps into `Jrpg.Data.Tests`, as `ArtSourcesTests` does.
- `ContentCatalogLoaderTests.Load_RepositoryContent_Succeeds` says the start layout refers to "potion"; once the maps are rebuilt (C2), restate the reason by an id or drop the comment.
- `SoundSourcesTests` reads its manifest with its own loose options (camelCase, unmapped members disallowed): a missing field reads as a default; switch it to `ContentJsonOptions.Strict` as `ArtSourcesManifest` does (AS1 review).
- The start map has no door into `interior_1` (decision 454): place one (a door object from the half-timber sheet, the event panel's door preset warping to `interior_1`), then remove the `Skip` on `RepositoryNewGameTests.StartMap_RepositoryContent_HasADoorIntoAnInterior`.

## From the object composer and animated tiles plan (2026-11-03, at approval)

- The editor's map animating (the plan's former AE4, decision 466): one shared, unfrozen `ImageBrush` per animated tile, swapped on `TileAnimationTime.FramesChanged`; `MapDrawing` paints those cells with it and leaves such a drawing unfrozen (design.md, "Extension points").
- The other out-of-scope items (an explicit frame list or per-frame times, per-cell start offsets, the tall grass effect, a free pixel rectangle from an image without a cut, animated canopies, an animated tile's mark on the sheet, the re-cut warning counting frames) are listed in `docs/plans/done/2026-11-03-object-composer-and-animated-tiles/README.md`, "Out of scope".
- `ObjectComposing.StartNew` clips a dragged rectangle to `ObjectCanvas.MaxCells` each way; only the row clip has a test, since no test-world sheet is wider than 32 cells (CO4a review): add a wide sheet to a test-only catalog when the composer is next touched.
- The "chosen cells' sheet has its image" query is written twice, in `Objects/Composing/CanvasPainting.PaintAt` and `Tiles/TileAnimationViewModel.CanOpen`: a third use moves it onto `TileBrowserViewModel` (AE1).

## From the object levels and sheet zoom plan (2026-11-05, at approval)

- Composer levels: showing or hiding a level, reordering levels, moving painted cells from one level to another (a level above an empty one came with the free levels plan, decision 490).
- A level with its own above-player zone or collision (ADR 0040's consequences: a format addition).
- The sheet zoom remembered between editor runs, keyboard shortcuts for it, steps below the sheet's own pixels; the Animate panel's preview following the zoom (decision 486).
- The marks' and grids' "one screen pixel" is one DIP, not exact at 125 % or 150 % Windows scaling (already true before the plan).

## From the object levels and sheet zoom plan (2026-11-05, during the slices)

- `Game/Rendering/ObjectSpriteBaker.Over` copies `Ui/UiTheme.Over` (`Rendering` doesn't reference `Ui`): a third user moves it into a shared rendering helper (OL2).

## From the music tracks change (2026-11-05, decision 496)

- The title's parts (intro, loop, outro) hand over when `MusicPlayer.FollowParts` sees the part's instance stopped, once per update: up to a frame of silence at each join, plus the device's start delay. If it is heard, a `DynamicSoundEffectInstance` fed the parts' samples in turn closes the gap (ADR 0035 amendment).
- The loops are 6 to 23 MB of 48 kHz WAV each, loaded whole by `SoundEffect.FromFile` on a map change, which can stall a frame: streaming through `DynamicSoundEffectInstance` is the ADR 0035 follow-up if it does.

## From the map editing flows plan (2026-11-05, during the slices)

- `tests/Jrpg.Editor.Tests/Shell/MainViewModelOpenMapsTests.cs` is at 286 lines after MS2 and `MainViewModelObjectFromMapTests.cs` at 251 after OM4: the next open-maps or object-from-map test goes in a sibling file.

## From the map fixes, banner and folders plan (2026-11-10, at approval)

- A per-placement depth offset for forced perspective on the player's layer (`depthOffset` on a placed object moving the line it sorts by), DO3 of the map fixes plan, if the developer ever hits the case: the developer's houses and trees sit on z -3 to -5, where the player is always in front, so the layer alone is the model.
- The Placed object panel's Depth offset box and the selected object's sort line drawn dashed, DO4 of the map fixes plan, with DO3, if the developer ever hits the case.
- `ContentWriter.RenameMap` (RN1): an `IOException` from deleting the old files after the writes leaves both maps on disk while the editor reports a failure and keeps the old content (ADR 0042 accepts the state; the editor could say which pair to delete).
- An undo or redo (Ctrl+Z) during a Shift + drag of the selection: `OnOpenMapChanged` calls `DrawSelectionMarks`, which snaps the marks back to the start as a zoom did before B3, and `_movedObject.Placed` then points at the object as it was before the undo (the B3 review, traced from the code, not reproduced).
- `MainViewModelMusicTests`' comment says `MainViewModelTests` is over 300 lines, which the cleanup plan's S3 made false (194); its two `StartEditingSettings_*` tests belong in `MainViewModelMapSettingsTests` (the S3 review).
- `MapDrawing.DrawCanopy` repeats `CollisionShapes.InMapCell`'s arithmetic times `CellPixels`: it could map through `CollisionShapes.InMapCells` and scale (the cleanup plan's R2 review).
- `MapDrawing.DrawObjectAt`'s NPC sprite branch has no test: `MapDrawingTests` never sets `FieldSpriteFile` (the R2 review).
- `PngSize.Read` reads the width and height with `ReadInt32BigEndian`, so a corrupt header of 2^31 or more comes back negative; the PNG rule forbids it, nothing checks it (the cleanup plan's R3 review).
- Files at the limit after the map fixes plan: `Shell/EditorTemplates.xaml` 297 and `Shell/MainWindow.xaml` 292 after the cinematics plan (Database > Cinematics...) (its status lines moved to `StatusBand` in the sprites plan's SI4; the Database menu took three lines); the next template goes in its own dictionary (as `EditorTreeTemplates.xaml` did) and the next control anywhere in the window in its own user control (as the Placed object panel did), never in these two.
- `CommandMenuTests.OpenItems` (Down twice) and `CommandMenuSkillTests`' `RecoveryRow`/`HealAllRow` still reach rows by a fixed count of presses, against the tests rule; older than the cleanup plan's GT2 (its review).
- Eight Game members read only inside their own file could be `private`: `TypingTicks.CharactersPerTick`, `OptionsPage.FullScreenRow`, `PromptPlacement.GapAboveObject`, `BattlerScaling.PaintingSize`, `ShatterLayout.RingShareMax`, `ResourceGaugeView.SinceChange`, `TurnOrderStrip.UpcomingCount`, `StatusSheetView.WriteLabel` (the Game cleanup plan's GX1 review).
- `verify.ps1 -Fix` on a tree that doesn't build lets `dotnet format` delete the toolkit's generator hooks (`partial void On...Changed`) as unused code (the cleanup plan's S5): make `-Fix` refuse to format until a plain build passes, or write the rule in CLAUDE.md (the S5 review).
- The map-replace expression `MapContent.With` holds (the cleanup plan's X1) is still written out in `ArtReferenceCheckTests`, `AudioReferenceCheckTests` and about 15 Editor, Game and Simulations tests that can't reach the Data.Tests helper: a shared test helper per project, or Core's catalog gaining the one-map replacement (the X1 review).

## From the character sprites and shops plans (2026-11-12, at approval)

- Class and skill balancing stays in `classes.json` and `skills.json`, checked by the startup validation, until their schema settles (the class and skill editors plan was dropped at approval); the first balancing tool worth building is a read-only side-by-side table of skills and classes (and enemies), not per-record forms.
- Replacing the player's field sprite with a bought sheet: its own plan, done with help (the character sprites plan only moves the player's drawing to its `field-sprites.json` record with today's numbers).
- A cutscene-moved NPC sorts against the player by its placed row (`MapDrawOrder` keys objects on `PlacedObject.Position`): a position lookup into the shared draw order, with the wandering NPCs plan.
- The sprite setup's `From` divides the image size by the record's frame size, so an image with spare pixels past its frames (which the start allows) opens with a cut problem until the counts are retyped (sprites plan SI2, SI4 reviews); bought sheets are exact, so it waits.
- Removing a placed NPC that another event's `moveNpc` step names is caught only at save (`MapLayoutValidator`); the editor could warn at the removal, as it does for an object in use (MV3's review).
- `Events/ListOrder`'s summary still says "the event dialog's lists" while the Quests and Shops windows use it too: reword it or move it to a shared folder (SH2's review).
- `ArtFolders.ImagesIn` (and the two older folder lookups) throws when a `--content` folder lacks the art folder asked for; the pickers could offer nothing instead (NP2's review).
- Six editor boxes parse a whole number their own way (`EventStepViewModel.WholeNumber`, `TypedCounts.Parse`, `TileAnimationViewModel.WholeNumberOf`, `PlacedObjectEditing.Layer`, `LayerChoiceViewModel`, `Catalogs/TypedNumber`): fold them onto one parser in a shared folder (EN2's review).
- `Shell/ArtStatus` refreshes the art warning only when the sheets or objects change: an Enemies window OK (or its undo) that renames a picture leaves the enemy-art lines stale until the next save (EN7's review).
- `Shell/MainViewModel.cs` is at 285 lines after the cinematics plan (`CinematicEditing` set beside its siblings); the drop table editor plan set `DropTableEditing` in the constructor beside its siblings (283 with its using, DT5, decision 605): the next window's editing view model is built in its own partial (as `MainViewModel.Database.cs`), not in the constructor's list.

## From the drop table editor plan (2026-11-13, at approval)

- **Duplicate in the Drop tables window** (cut at approval, decision 606): a new table copying the chosen one's rows as typed (the boxes, never `ToTable()`, which throws on a typed problem), its id to type, as the Enemies window's Duplicate (decision 585).
- **An average-copies-per-defeat line in the Drop tables window** (cut at approval, decision 606): if it comes back, the average (Σ chance × average quantity, before drop luck) is a `LootFormula` function beside the chance rule, not a method on `DropTable`.
- **Drop chances finer than whole percents** (decision 608): `DropEntry.Chance` in permille, for drops rarer than 1 in 100: Core's record and `LootFormula.ChancePermille`, Data's validator and the file's `chance` field, the Drop tables window's box; no save change (saves never name a table).
- **The catalog list view model is copied three times**: `Trade/ShopCatalogViewModel`, `Battle/EnemyCatalogViewModel` and `Loot/DropTableCatalogViewModel` each hold the records in id order, remove only an added one, watch each record's typed fields and raise `Problem` and `CanApply` together (DT4 review). The shared rule is already `RecordListProblem`; what is left is per-type event wiring, so a shared base needs a design of its own.
- **A cinematic still panning toward a point** (decision 618): the zoom of the cinematics plan scales about the still's center only; a pan would add a target point per still (Core's zoom, Data's field, the still's editor boxes).
- **Audio in the Cinematics window's preview** (decision 620): Play is silent and each still shows its voice's length instead; playing voices and the track would need the editor to mirror the game's music rules (intro, loop, outro).
- `Editor/Events/EventStepViewModel.cs` is at 260 lines after the cinematics plan (CM3 took it to 292, CR3 removed the still's fields): the next step kind past about 290 moves the `Label` and `Problem` switches into a sibling type first. (CM3 review, cinematics retro)
- The editor's list view models' row watch (unwatch the old rows, watch the new, in that order for a Move) is shared as `Catalogs/ListItemWatch.Follow` from the cinematics plan's CE4: `QuestViewModel.OnStagesChanged` and `DropTableViewModel.OnEntriesListChanged` still copy it; switch them with the next change to either. (CE4 review)

## From the battle turn presentation plan (2026-11-14, at approval)

- **Projectiles** (decision 665): an effect travelling from the attacker to its target, as a field on `EffectStripDefinition` beside `SwingFile` (a strip and a travel time, read by `ActionWindUp` as the swing is). Skills' and items' own strips are built (`effectFile`, the skill effect strips plan); a skill's own swing comes from its strip's record.
- **A baked soft glow for the actor's blink** (decision 648): if FL1's cheap spill (the white silhouette drawn 8 times around the sprite) isn't enough, a `Ui/GlowMask` (the silhouette's alpha padded and blurred, frame by frame so an animated strip's frames don't bleed) baked once per battler by `UiTextures`.
- **Per-frame times in an effect strip** (decision 632): the golden impact's fast streak and slow sparks share one frame time; a list of times on `EffectStripDefinition` would let each phase keep its own.
- **Remove `AttackEnemy.png` and `AttackPlayer.png`** (decision 664) and their `art-sources.json` lines once the golden art is final and nothing names them.
- **Per-action opening and impact sound files** (decision 647): the developer will add them to skills and items; `Game/Battle/ActionSounds` reads them before today's defaults (its `ImpactOf` takes the command then).
- The frozen test world's `cinematics.json` (cinematics plan's CR2) points at the live `Placeholder1.png` to `Placeholder3.png` in `content/art/cinematics/`: when real art replaces them, freeze the three pictures under `tests/TestWorld/art/cinematics/` or `TestWorldTests.FindProblems_TestWorld_FindsNone` breaks. (CR2 review)
- `Ui/CoverFit.DrawOverScreen` draws what `DrawZoomedOverScreen(..., 1)` draws, and both repeat the `SourceIn` line (the cinematics plan's CG6): make the first call the second with the next change to either. (CG6, CR3 reviews)
- The Game's cinematic presenter tests copy their fixed-step helpers: `Run(presenter, time)` in `EventPresenterCinematicTests`, `...VoiceTests` and `...SkipTests`, `StartPastTheLeadIn` in the same three, and `HoldConfirmToSkip` in `EventPresenterTests` and `GameFlowNewGameTests`: move them into `EventPresenters` (or a `CinematicSteps` test helper) with the next test that needs one. (cinematics retro)

## From the animated monsters plan (2026-11-14, out of scope)

- **The party's battle animation** (decision 641): move right or left and an idle loop for party members; a member's
  loop would be the enemy's `idleAnimation` field on `PartyMemberDefinition`.
- **A monster's own attack or hit animation**, and a skill's own effect strip (skills name no effect today).
- **Per-frame times in a battler's loop** (decision 632): one frame time per loop, as ADR 0039, so a GIF whose delays
  vary plays evenly.
- `Editor/GifImport/GifFrame` is a record holding a `uint[]`: two frames compare their pixels by reference, and `with`
  shares the array (MB3's review; its summary says so). A test asserting two frames equal would fail for no visible
  reason: a plain class, or equality over the pixels, if one ever compares them.
- **The gnolls' stats, tactics, drops and encounters** (decision 640): the developer's content, in Database >
  Enemies... after MB5, with an encounter in `encounters.json` kept off the start area's random table.

## From the animated monsters plan's retro (2026-11-14)

- `Editor/Battle/EnemyCatalogWindow.xaml.cs` grew 114 → 202 lines in this plan (the strip preview, the GIF options):
  the next Art tab feature moves the battle sprite preview (`ShowFrame`, `FrameOf`, `SizeLineOf`) into its own type first.
- `Game/Battle/BattlersView.FrameRectangle` cuts a strip's frame from the drawn size, which equals the frame's own size
  only because `BattlerScaling.AtOwnSize` never scales (MB7's review): pass the frame size if the drawn size ever differs.

## From the battle turn presentation plan's phase 3 (2026-11-15)

- `Game/Battle/SwingView` and `ImpactEffectsView` each keep a per-image `EffectStrip` cache (`PaceOf`) and draw a square
  frame centered on a point the same way (EF5's review): when a third view needs them (a projectile, decision 665),
  share them in one place first.
- `ImpactEffectsView.DrawStrip` takes a `(File, Target, IsMirrored)` tuple beside `EffectOnTarget` (EF4's review): the
  wind-up could build its lead as `EffectOnTarget`s once.
- `Shell/MainViewModel.Database.CreateEffectStripEditing` and `MainViewModel.FieldSprites.CreateFieldSpriteEditing` are
  the same readable-image listing and size lookup over two folders (EE3's review): a third copy takes one shared pair.
- "The playback whose damage landed" (`_sequence.Playback is { Strips.Count: > 0 }`) is read three times: in
  `ImpactLightView.LitImpactNow`, `BattlersView.WashAt` and `BattlersView.DrawWhite` (the battle presentation plan's
  IL1-IL3): one `BattleTurnSequence` query.
- `Game/Battle/BattlersView.cs` is at 257 lines after the impact's light, its white layers (the blink's spill, the
  fallen's and the wash's white, the fringe's split) drawn in place: a `BattlerWhiteLayers` drawer would take them.
- `MainViewModel.UnsavedChanges` doesn't count a stroke still being dragged: the close question ends the stroke first,
  the unexpected-error box doesn't, so a throw mid-drag on a clean map reads "Nothing is unsaved." (NET1's review;
  nothing is lost, closing still asks).
- For the editor push's row 7 (the shared animation preview): per-facing frame counters ("2/4") in the Character
  sprites window and the playing cell outlined on the sheet (cut from the sprite cells plan at its review).
- `ObjectTile`'s and `GridCell`'s members aren't required when read: a missing `x` reads as 0 (the serializer builds a
  struct through its parameterless constructor); add them to the sprite cells plan's `RequiredStructMembers` list.
- `Editor/Sprites/FieldSpriteSetupViewModel.PreviewFrameAt` (formerly `PreviewCellAt`) has no caller but its tests:
  the walk preview reads the record itself; remove it or give it the preview (sprite cells plan, SC3 review).
- `Data/Effects/EffectValidator.FindDamageTypeProblem` names enums in their C# spelling ("Fire on RestoreHp") where
  the newer `FindTierProblem` uses the file's (`ContentJsonOptions.EnumName`); spell them the same (skills and statuses
  plan, T2 review).
- Editor marks drawing copied many times: the `Frozen(Brush)` helper (`SequenceMarks`, `SpriteSheetMarks`,
  `CollisionDrawing`, `TilesheetMarks`' `Frozen<T>`) and the clipped `DrawingGroup` pinned by a transparent rectangle
  and frozen (six files); one shared marks-drawing type could hold both (sprite cells plan, SW3 review).
- `Data/Effects/EffectValidator.FindStatusesProblem` refuses one status id twice in an effect, not two records of one
  family (`guard_1` and `guard_2`), which then refresh each other on landing; decision 798 allows it today: compare
  `FamilyKey` if the developer wants that refused (skills and statuses plan, SW review).
- `Game/Menus` Status page: its skills section (268 px) holds three two-line skills, so no class can have a fourth;
  Enfeeble and Cone of Cold wait in `skills.json` off their classes' `skillIds` until the section scrolls or pages
  (skills and statuses plan, WK; needed before row 12's classes and progression).
- `Editor/Quests/QuestCatalogViewModel.OnQuestChanged` raises `Problem` on every quest notice, its own `Label`,
  `Problem` and `SelectedStage` included, where the editor rules ask for the typed fields only; cheap today (no Data
  check) (mode bar plan, WD3).
- `.claude/rules/editor.md`'s dialog line says OK greys "through `CanApply`"; a page in `Workspaces/CatalogDialog`
  greys it on `IEditedCatalog.Problem`: name both at the mode bar plan's retro (WD2, WD3 reviews).
- `Editor/Cinematics/CinematicCatalogPage`'s Music box shows the previous cinematic's track when the chosen one's
  track is missing from its folder (WPF keeps a stale display; `MissingNote` names the file): list the missing tracks
  after the folder's, as the Skills page does since SK4 (skill and status editors plan, SK4 review).
- `X.Insert(SelectedX is null ? X.Count : X.IndexOf(SelectedX) + 1, row)`, "insert after the chosen row": since
  SK7 `Events/ListOrder.InsertAfter` does it (the Skills page's costs and grants); five copies remain to replace with it
  (Cinematics, Events, Loot, Quests, Trade) (skill and status editors plan, SK5 review).
- The Effects page's Kind box and list (`Effects/TieredEffectViewModel.Kinds`) and the Statuses page's part Kind box
  (`Statuses/StatusPartRow.Kinds`) show the enums' C# names (`RestoreHp`, `DamageEachTurn`) where the files say
  `restoreHp`, `damageEachTurn`: one choice type or display converter spelling them as the files do would serve both
  (skill and status editors plan, FX2b and FX6 reviews).
- `Editor/Battle/EffectStripViewModel` (`LengthText`, `IsListed`, `ToStrip`, `HoldsTheLoadedRecord`,
  `HitFrameProblem`) still parses its texts with `TypedNumber.Parse` where AN1b's `FrameMilliseconds` and `HitFrame`
  hold the numbers (skill and status editors plan, AN1b review).
- `Editor/GifImport/GifConversion.Prepared` divides by a 0 height in `FrameScaler.WidthAt` when a GIF's screen
  descriptor says 0 x 0: refuse it as `ComposedGif.Read` does since AN2 (AN2 review).
- An effect image replaced with fewer frames, then the Effect strips page cancelled, leaves the draft's record with a
  hit frame past the new strip, which the game's start refuses (`ArtReferenceCheck.AddHitFramesPastTheirStrip`; the
  art warning shows it): say so on the import line when a replaced strip's record no longer fits (AN4 review).
- `Editor/Shell/MainViewModel.SheetImport.cs` keeps its own replace question ("tilesheet art", the folder being
  `tilesheets`) and its own refresh and status report, which AN7's `ConfirmsReplacing` and `ReportArtImport` now
  serve for the art images (AN7 review).
- `tests/Jrpg.Editor.Tests/Battle/EnemyCatalogViewModelTests.cs` is at 299 lines: split it (the problem-line tests to
  their own file) before the next test lands there (enemy behavior plan, EB6 and EB7 reviews).
- Ten editor views declare their own `BooleanToVisibilityConverter x:Key="VisibleWhenTrue"`: one key in
  `Shell/EditorTheme.xaml` would serve them all (EB7 review).
- Five Data test files (the enemy validator's and writer's, EB9's `EnemyTacticConditionValidatorTests` among them)
  each build their own `_goblin` fixture: one shared test enemy in `Jrpg.Data.Tests` would serve them (EB9).
- Ten Core records write their `Equals` by hand (`ItemDefinition`, `DropTable`, `Effect`, `StatusDefinition`,
  `QuestDefinition`, `CinematicDefinition`...): a member added later and left out passes every test and breaks the
  draft's change detection; one shared reflection test (each init property changed alone with `with` must make the
  records unequal) would guard them all (items plan, IU2 review).
- `ItemDefinitionBuilder` has no `AsUnique(...)`: the `with { IsUnique = true, FixedAffixes = [...] }` block is
  copied in about fifteen test files across Core and Data (items plan, IU2-IU4 reviews).
- `Editor/Catalogs/RecordListProblem.FirstOf` counts two new records with empty ids as one id twice ("Item  is listed
  twice.", ahead of their own "The id must not be empty."): skip empty ids in the twice check (items plan, IU6 review).
- A new game or a load builds a new `Game/Exploration/ExplorationScene` with nothing decoded, so the last playthrough's
  map backgrounds stay in `TextureLibrary` until the session ends (bounded by the background files): release them
  when the session ends a playthrough (encounters and backgrounds plan, BG5 review).
- `Editor/Tiles/SheetImportResult` now carries battle-background records too (so `Tiles` uses `Core.Battle`); it serves
  every art import, so `Catalogs/` is its home (encounters and backgrounds plan, BG9 review).
- `Editor/Battle/GifImportWindow.AskFor` tests a path for `.gif` with its own `Path.GetExtension` check, a copy of
  `Catalogs/ArtImageImport.IsGif`: share one (encounters and backgrounds plan, BG9b review).
- `Data/Content/CatalogProblems.OfEnemies` doesn't run the `encounters.json` check: removing an enemy an encounter names
  shows no problem on the Enemies page and Save then refuses naming encounters.json; run every check that reads
  `enemies.json`, as `CheckItems` does for items (encounters and backgrounds plan, EC1 review). Unreachable today: the
  Enemies page removes only an enemy added in its session (EC2 review); it matters once a saved enemy can be removed.
- The page-local `FieldLabel` and `NumberBox` styles are copied in about ten Database pages (Enemy, Cinematic,
  TieredEffect, Encounter, Item, Affix, DropTable, Class...): one keyed style each in `Shell/EditorTheme.xaml` would
  replace them (classes and progression plan, CL4 review).
- `Core/Stats/StatFormula.MaxHp` casts a long to int and `AccuracyPermille` multiplies an int by 10: on absurd typed
  growth (e.g. max HP growth 100,000,000 over 21 levels) Constitution's share wraps negative and `MemberSheet` shows
  max HP 1 instead of the Levels tab's "too large" problem; cap both at int's bounds in one place, with a test (classes
  and progression plan, CL5 review).
- About 29 Core test files each hold a `private static SkillDefinition Skill(string id)` factory: one shared builder in
  `tests/Jrpg.Core.Tests/Builders/` would replace them (classes and progression plan, LV2 review).
- The eight class stat labels in page order are written twice, in `Editor/Party/ClassStatsBoxes.Problem` and
  `ClassLevelPreview.StatsOf`: share one ordered list at a third copy (classes and progression plan, CL5 and LV2 reviews).
- The victory screen's learned-skill lines (`Battle/VictoryLearnedSkillsView`) stack under the cards: with five
  members learning three or more skills in one battle, the lines from the third on cross the footer's hairline
  (`OutcomeFrame.FooterLineTop`, 910) and go under the footer bar. The developer's options: the empty slot beside the
  fifth card, a cap with "and N more", or a dialog (decision 964's alternative). It can't happen with today's content,
  every class skill being at level 1 (classes and progression plan, LV3 review).
- `Editor/Encounters/EnemyChoice` and `Loot/DropItemChoice` are the shape of `Catalogs/RecordChoice` (an id, a name,
  "Name (id)"): replace them when next touched; `Trade/PricedItemChoice`, `Skills/StatusChoice` and
  `Battle/TacticSkillChoice` each carry an extra and stay (classes and progression plan, CL6, decision 967).
