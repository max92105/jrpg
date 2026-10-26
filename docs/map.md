# Codebase map

Where things are and the patterns they follow. Read this before exploring the code; open files only for what it doesn't
answer. One line per feature folder: what it's for, then its key types. Keep it short: when a slice adds a folder or a
type other code will build on, add or edit its line here (the `/slice` skill does this). Folder = namespace.

## Patterns used everywhere

- **Rules in Core, shells around it** (ADR 0001). Any `if` that decides gameplay is in `Jrpg.Core`, with a test. Game and Editor read Core state and send it commands.
- **Core reports what happened as values**: battle turns return `BattleEvent`s (ADR 0004), map events run step by step through `EventRun` (ADR 0006), item use returns results. The Game animates them; tests assert them.
- **Content only in `content/`** (ADR 0011), loaded and validated by Data into immutable Core records (ADR 0002); player-facing text in `content/text/ui.json` through `MessageIds` (ADR 0010).
- **Scenes return requests** (ADR 0008): `IScene.Update` returns a `SceneRequest` or null; `GameFlow` applies it through `SessionTransitions` / `MenuTransitions`, behind its `ScreenFade`. A scene never builds another; `SceneFactory` builds them.
- **Pure presenter + thin view**: each screen's state and input handling is a plain class tested headless in `tests/Jrpg.Game.Tests` (`TitleMenu`, `ListCursor`, `SpeechPlayback`, `EquipmentChoice`); a `...View` or the page's `Draw` only draws it.
- **No allocation per frame**: text is formatted and measured when state changes, never in `Draw`; textures are built once (`UiTextures`, `MapArt`, `TextureLibrary.Create`).
- **Determinism**: randomness through `IRandom` (`SeededRandom`), time through the update's delta; the only wall clock is the save stamp in `Program`.
- **Editor edits are commands**: `IMapEdit` applied to a `MapDraft`, recorded in `EditHistory` for undo (ADR 0012); MVVM with CommunityToolkit.

## Jrpg.Core (rules, no UI)

| Folder | Purpose | Key types |
|---|---|---|
| Battle | One battle: turn order, commands, events, enemy AI, rewards | `BattleState`, `BattleCommand`, `BattleEvent`, `TurnOrder`, `Formation` (the party's line in one battle, ADR 0023; Move swaps neighbours), `TurnFlow` (from one turn to the next: upkeep, outcome, advance past the stunned; a quick skill keeps the turn; no upkeep on an extra turn), `TurnOrder` holds readied extra turns (RG3; `UpcomingTurn` flags them in `BattleState.UpcomingTurns`), `TurnUpkeep` (the end of the holder's own turn: per-turn gains, status countdown), `SkillPayment` (resource requirements and costs: can pay, what's short, pay), `ResourceTriggers` (gains that happenings trigger: a hit taken or landed, a heavy hit, an ally or an enemy falling), `EffectLanding` (built with the combatants; an effect on its targets: each hit, the status granted, a fall), `ResourceStarts` (a member's resource starts for a battle, lasting ones from `PartyMember.LastingAmounts`), `LastingResourceCarry` (a finished battle's lasting amounts back onto the members, ADR 0024), `SkillBackfire` (the roll that turns a spell on its caster), `SkillEmpowerment` (a skill's effect raised by its user's dominant force), `EnemyTactics` (a random living member, or a taunting one), `VictoryRewards` |
| Content | The loaded catalog and global rules | `ContentCatalog`, `GameRules`, `NewGameDefinition`, `LeaderChoice` |
| Dialogue | What a speaker says | `Speech` |
| Effects | What skills and items do, targeting, damage formula | `Effect`, `EffectResolver`, `Targeting`, `TargetChoice`, `DamageFormula` |
| Events | Map event definitions and their steps | `MapEvent`, `EventStep` (+ `DialogueStep`, `GiveItemStep`, `SwapObjectStep`, `WarpStep`, `StillStep`, `VoiceStep`, `RecruitStep`, `RestStep`; presentation steps are skippable) |
| Exploration | The player on the map: walking, collision, events (on interact, touch or arrival), random encounters, what Confirm would start | `ExplorationState` (takes the party, which recruit steps add to; `Recruitment` says what one did), `PlayerWalker`, `MapCollision`, `EventRun`, `ArrivalEvents`, `RandomEncounters`, `InteractionTarget` |
| Geometry | Positions and shapes | `Point2D`, `GridCell`, `Facing`, `Polygon` |
| Items | Items and the inventory | `ItemDefinition`, `Inventory`, `EquipmentSlot` |
| Maps | Map content and draw order | `MapDefinition`, `MapLayout`, `PlacedObject`, `ObjectDefinition`, `DrawOrderKey`, `WalkabilityGrid` |
| Party | Members, the active party, equipment (taking it from another member too), field item use | `PartyState`, `PartyMember` (stats from its class at its level, ADR 0017; joins at level 1), `ClassDefinition` (also the skills every member of it has, ADR 0020; read them through `PartyMember.Skills`), `ClassStats`, `ClassGrowth` (a class's stats at a level, ADR 0017), `PartyEquipment`, `WornItem`, `FieldItemUse` |
| Progression | Experience and levels | `ExperienceTable`, `LevelProgressStep` |
| Randomness | Seeded randomness | `IRandom`, `SeededRandom` |
| Resources | Class battle resources (ADR 0019): definitions, gains per trigger, one combatant's amounts in a battle | `ResourceDefinition`, `ResourceGains`, `ResourceChange` (what a skill does to one resource), `ResourceRequirement` (a minimum a skill needs, never spent), `HeavyHitRule` (a hit of at least a share of max HP), `ResourceStart` (a max and start for one battle), `SpeedScaling` and `ReadinessFormula` (charges from speed), `BackfireRule` and `BackfireFormula` (a resource's danger), `DominanceRule` and `DominanceFormula` (rival forces), `StrainRule` (a lasting resource lowering another's start), `ResourceMeters` (on `Combatant.Resources`, built new each battle; tracks which force dominates; changes reported as `ResourceChanged`) |
| Saving / Session / World | The save record; one playthrough (starts and finishes its battles); what the player changed in the world | `SaveGame`, `GameSession`, `WorldState` |
| Skills / Stats | Skill definitions; stats and HP | `SkillDefinition`, `CombatStats`, `Vitals` |
| Statuses | Battle statuses (ADR 0021): the closed list of kinds, what an effect grants, one combatant's set | `StatusKind`, `StatusGrant`, `ActiveStatus`, `StatusSet` (on `Combatant.Statuses`; fresh until the turn it was granted on ends), `StatusModifiers` (what statuses do to stats, `Combatant.CurrentStats`, and to damage taken); Poison and Regen tick in `Battle/TurnUpkeep.TickStatuses`, reported as `StatusTicked` |

## Jrpg.Data (content and saves on disk)

| Folder | Purpose | Key types |
|---|---|---|
| Content | Loading, validating and writing the catalog; where content is; an optional field added later is left out of a file while null (`AbsentWhenNullFilter`) | `ContentCatalogLoader`, `ContentWriter`, `ContentRootLocator`, `ContentValidationException` |
| Art | Art folders, the field sprite frame grid and the startup art check | `ArtFolders`, `FieldSpriteLayout`, `ArtReferenceCheck`, `PngSize` |
| Audio | Audio folders and the startup check that every voice file exists by exact name (ADR 0018) | `AudioFolders`, `AudioReferenceCheck`; both checks share `Content/ExactFileName` and `Maps/MapEventSteps` |
| Maps, Items, Party, Skills, Battle, Effects, Resources, Statuses | One validator per content type (a class's resources by `ResourceDefinitionValidator`, called from the class one) | `MapLayoutValidator`, `ItemCatalogValidator`, … |
| Messages | The UI message table | `UiMessageLoader` |
| Saving | Save files: versioned JSON (an older or newer format is refused until release, ADR 0022), atomic writes, repair against changed content | `FileSaveStore`, `SaveGameRepair`, `RepairedSave` |

## Jrpg.Game (MonoGame shell)

| Folder | Purpose | Key types |
|---|---|---|
| (root) | Startup and the host loop | `Program`, `JrpgGame` |
| Flow | The screens and the playthrough; applies scene requests | `GameFlow`, `FadeGate`, `SceneFactory`, `SessionTransitions`, `MenuTransitions`, `BattleTransitions` |
| Scenes | The scene contract, the stack, requests, the screen fade | `IScene`, `SceneStack`, `SceneRequest` (+ records, `BattleRequested`, `BattleFinished`, `LeaderChosen`), `ScreenFade` |
| Hosting | Window, settings, file locations | `PlayerSettings`, `TextSpeed`, `GamePaths`, `FullScreenShortcut`, `DeveloperLog` (the console and debug output), `CaptureFiles` (Debug: F12 saves the 1920 x 1080 screen to `docs/captures`; other builds `%LocalAppData%/Jrpg/Captures`) |
| Audio | The voice-over path (ADR 0018): scenes write the channel, the host plays it after each update; no device means silent | `VoiceChannel`, `SoundLibrary`, `VoicePlayer` |
| Input | Keys and pads to game buttons, per update; the device in use and its button names | `InputReader`, `InputFrame`, `InputBindings`, `GameButton`, `InputDevice`, `ButtonLabels` |
| Rendering | Textures, fonts, the virtual screen, baked map art | `TextureLibrary`, `FontLibrary` (body and title faces, ADR 0025), `VirtualScreen`, `MapArt`, `TileAtlas` |
| Exploration | The map scene: drawing (NPC objects from their field sprite), camera, walking, showing events (cutscene stills included), the interaction prompt | `ExplorationScene`, `EventPresenter`, `StillRun` (a run of stills, opening and closing on black), `StillPlayback`, `StillView` (a still with its subtitle band and skip hint), `MapView`, `MapDrawList`, `MapCamera`, `FieldSpriteFrame`, `InteractionPromptView`, `PromptPlacement`, `PromptFade` |
| Dialogue | The message box and typing | `DialogueBox`, `SpeechPlayback`, `Typewriter`, `FoundItemText` |
| Title | The title screen | `TitleScene`, `TitleMenu`, `TitleEntrance` (the fade in from black) |
| NewGame | The steps of a new game before the map: the leader choice | `LeaderChoiceScene`, `LeaderChoiceMenu`, `LeaderProfile` (what the choice shows of a character, built once), `LeaderProfileView` (draws it: portrait, texts, stats; fades in on a new character) |
| Battle | The battle screen over the map: turns, commands, playback (ADR 0013) | `BattleScene`, `BattleTurnSequence`, `TurnStage`, `BattlePacing`, `ActionPlayback`, `PlaybackBeat`, `BattleBanner`, `BattleBannerView`, `FloatingNumber` (with `FloatingNumberTone`: damage, heal or Poison, which sets its color), `FloatingNumbersView`, `EffectStrip`, `PlaceholderCue`, `ImpactEffectsView`, `BattleLayout` (spots: the party by its place in the formation), `FormationSlide` (battlers and panels easing to new places after a swap), `BattlerArt`, `BattlerScaling`, `BattlersView`, `VitalsText`, `ResourceGaugeView` (a member's resources as pips in their color, spaced-caps names, the lasting marker, the dominant force lit, a flash on change), `StatusMarksView` and `StatusMark` (a combatant's statuses as small diamond marks with turns left, rimmed red or blue, popping in and fading out), `StatusNames` (each status's name id), `PartyStatusView`, `BattleVitalRow`, `EnemyLabels`, `EnemyTagView`, `TurnOrderStrip`, `TurnOrderView`, `TurnDiamondView`, `VictoryScreen`, `VictoryScreenView`, `VictoryEntrance`, `EscapedNotice`, `DefeatScreen`, `DefeatScreenView`, `OutcomeFrame`, `DefeatEntrance`, `DefeatMenu`, `DefeatEntry`, `BattleSceneSources`, `LevelUpPop`, `VictoryRewardRow`, `ExperienceBarAnimation`, `CommandMenu`, `CommandEntry`, `BattleSkillChoice`, `SkillCostText` (a skill's cost or requirement line and its "not enough" line), `BattleItemChoice`, `IBattleListChoice`, `CommandRowLabel`, `BattleTargetCursor`, `CommandPanelView`, `CommandPanelPlacement` |
| Menus | The game menu and its pages | `GameMenuScene`, `MenuMotion` (the slide-in and the closing fade), `IMenuPage`, `IMemberPage`, `StatusPage`, `StatusSheet`/`StatusSheetView`/`StatusLabels`, `InventoryPage`, `InventoryRow`/`InventoryListView`/`ItemDetailLine`, `CategoryRail`/`CategoryRailView`/`CategoryTab`, `EquipmentPage`, `EquipmentSheet`/`EquipmentSheetView`, `EquipmentChoice`/`EquipmentChoiceView`, `StatPreviewText`, `PartyPage`, `PartyCursor`, `PartyColumns`/`PartyColumnsView`, `PartyActionChoiceView`, `OptionsPage`, `SystemPage`, `PageTransition`, `MenuFrame`, `PartySidebar`/`SidebarCard`/`PartySidebarView` |
| Saving | The 20 save files and their list | `SaveSlots`, `SaveSlotList`, `SaveFilesPanel` |
| Ui | Shared UI pieces: theme, text, cursors, panels, bars, shapes, easing | `UiTheme` (partial: shared, `.Dialogue`, `.Battle`), `UiTextures`, `UiMessages`/`MessageIds`, `ListCursor`, `MenuPointer`, `PointerBob` (a pointer's bob, any distance), `SelectionGlide` (a list's highlight gliding between rows), `HeaderBar`, `StatBar` (a fill tint turns one grey gradient into any color), `PipRow`/`PipRowLook`/`PipFlash` (diamond pips, lit or dim, flashing when changed), `SectionTitle`, `MeasuredText`, `NumberText`, `ColumnTitles`/`ColumnTitle`, `FooterBar`, `VitalMeter`, `MemberVitalBars`, `MemberCardView`, `Diamond`, `ConfirmPrompt`/`ConfirmPromptView` (a centered titled window), `FramedWindowLayout`/`FramedWindowView` (the reference's double-hairline window, untitled or titled), `Vignette`, `OutlinedFont`, `DiamondShape`, `OutlinedShape`, `CommandHeaderShape`, `PointedBarShape`, `Easing` |

## Jrpg.Editor (WPF tools)

| Folder | Purpose | Key types |
|---|---|---|
| Shell | The main window, its menu bar and editing modes; one undo order over the map and the catalogs (`CombinedHistory` over `UndoJournal`); F1 lists the controls (a new gesture adds its line there) | `MainWindow`, `MainViewModel`, `EditingMode`, `MapEntry`, `ControlsHelpWindow`, `CombinedHistory`, `UndoJournal`, `EditorTheme.xaml` (the dark theme: every look value of a view, by role; every window sets its `EditorWindow` style), `EditorTemplates.xaml` (dark templates for combo boxes, scroll bars, tool bars and menus) |
| Maps / Maps/Editing / Maps/Settings | The map document, its undoable edits, map settings and new maps; the collision overlay, from Core's walkability rule | `MapViewModel`, `MapDraft`, `IMapEdit`, `EditHistory`, `MapSettingsViewModel`, `LayerChoiceViewModel`, `LayerEntry`, `CollisionShapes`, `CollisionDrawing`, `PlacedObjectHits` (which placed object a click or a rectangle hits; an NPC by its whole cells) |
| Catalogs | The content the editor works on, read by every panel and the drawing; catalog changes go through it as undo steps, each a `CatalogEdit` of two functions; an undo removing a record still in use is refused (`CatalogUsage`; ADR 0014); a re-cut names what it touches (`SheetUsage`) | `CatalogDraft`, `CatalogEdit`, `CatalogHistory`, `CatalogUsage`, `SheetUsage`, `SheetCell`, `SheetCellRectangle` (cells chosen with Shift + drag) |
| Tiles, Objects | Tile and object browsers, painting and placing; marking tiles blocked; the sheet box lists every tilesheet image, set up or not (`SheetChoice`); Set up opens the Cut panel (columns and rows in, tile size out; guessed tile size, id from the file name); Import copies PNGs into the tilesheet art through Data (`SheetImport`) | `TilePaintingViewModel`, `TileBrowserViewModel`, `SheetChoice`, `SheetImages`, `SheetCutViewModel`, `SheetCutGuess`, `TilesheetIds`, `SheetImport`, `WalkabilityMarking`, `ObjectPlacingViewModel`, `ObjectBrowserViewModel`, `ObjectShapesViewModel`/`ObjectShapesWindow` (an object's collision outline and above-player zones, applied with `CatalogDraft.SetObject`; `ShapeEditing` finds the handle or edge under the mouse), `NewObjectViewModel`, `ObjectFromCells`, `ObjectIds` (a new object from sheet cells), `NewNpcViewModel`/`NewNpcWindow` (a new NPC object from a field sprite, ADR 0015) |
| Events | Object and cell event dialogs and presets | `EventEditingViewModel`, `EventStepViewModel`, `EventPresets` |
| Viewport | Zoom and pan | `ViewportTransform` |

## Where to look

- A rule or formula → Core, its folder above; formulas sit in one static class per domain (`DamageFormula`, `ExperienceTable`).
- A content field → the record in Core, its validator in Data, the JSON in `content/`.
- A screen → `Jrpg.Game/<Feature>`; its tests in `tests/Jrpg.Game.Tests/<Feature>`.
- Text the player reads → `content/text/ui.json` and `Ui/MessageIds`.
- A design decision → `docs/decisions/README.md`; a feature's plan → `docs/plans/README.md`.
