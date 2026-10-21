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
| Battle | One battle: turn order, commands, events, enemy AI, rewards | `BattleState`, `BattleCommand`, `BattleEvent`, `TurnOrder`, `EnemyTactics`, `VictoryRewards` |
| Content | The loaded catalog and global rules | `ContentCatalog`, `GameRules`, `NewGameDefinition` |
| Dialogue | What a speaker says | `Speech` |
| Effects | What skills and items do, targeting, damage formula | `Effect`, `EffectResolver`, `Targeting`, `TargetChoice`, `DamageFormula` |
| Events | Map event definitions and their steps | `MapEvent`, `EventStep` (+ `DialogueStep`, `GiveItemStep`, `SwapObjectStep`, `WarpStep`) |
| Exploration | The player on the map: walking, collision, events, random encounters | `ExplorationState`, `PlayerWalker`, `MapCollision`, `EventRun`, `RandomEncounters` |
| Geometry | Positions and shapes | `Point2D`, `GridCell`, `Facing`, `Polygon` |
| Items | Items and the inventory | `ItemDefinition`, `Inventory`, `EquipmentSlot` |
| Maps | Map content and draw order | `MapDefinition`, `MapLayout`, `PlacedObject`, `ObjectDefinition`, `DrawOrderKey`, `WalkabilityGrid` |
| Party | Members, the active party, equipment, field item use | `PartyState`, `PartyMember`, `PartyEquipment`, `FieldItemUse` |
| Progression | Experience and levels | `ExperienceTable`, `LevelProgressStep` |
| Randomness | Seeded randomness | `IRandom`, `SeededRandom` |
| Saving / Session / World | The save record; one playthrough (starts and finishes its battles); what the player changed in the world | `SaveGame`, `GameSession`, `WorldState` |
| Skills / Stats | Skill definitions; stats and HP/MP | `SkillDefinition`, `CombatStats`, `Vitals` |

## Jrpg.Data (content and saves on disk)

| Folder | Purpose | Key types |
|---|---|---|
| Content | Loading, validating and writing the catalog; where content is | `ContentCatalogLoader`, `ContentWriter`, `ContentRootLocator`, `ContentValidationException` |
| Art | Art folders and the startup art check | `ArtFolders`, `ArtReferenceCheck`, `PngSize` |
| Maps, Items, Party, Skills, Battle, Effects | One validator per content type | `MapLayoutValidator`, `ItemCatalogValidator`, … |
| Messages | The UI message table | `UiMessageLoader` |
| Saving | Save files: versioned JSON, atomic writes, repair of old or damaged saves | `FileSaveStore`, `SaveGameRepair`, `RepairedSave` |

## Jrpg.Game (MonoGame shell)

| Folder | Purpose | Key types |
|---|---|---|
| (root) | Startup and the host loop | `Program`, `JrpgGame` |
| Flow | The screens and the playthrough; applies scene requests | `GameFlow`, `FadeGate`, `SceneFactory`, `SessionTransitions`, `MenuTransitions`, `BattleTransitions` |
| Scenes | The scene contract, the stack, requests, the screen fade | `IScene`, `SceneStack`, `SceneRequest` (+ records, `BattleRequested`, `BattleFinished`), `ScreenFade` |
| Hosting | Window, settings, file locations | `PlayerSettings`, `TextSpeed`, `GamePaths`, `FullScreenShortcut` |
| Input | Keys and pads to game buttons, per update | `InputReader`, `InputFrame`, `InputBindings`, `GameButton` |
| Rendering | Textures, fonts, the virtual screen, baked map art | `TextureLibrary`, `FontLibrary`, `VirtualScreen`, `MapArt`, `TileAtlas` |
| Exploration | The map scene: drawing, camera, walking, showing events | `ExplorationScene`, `EventPresenter`, `MapView`, `MapDrawList`, `MapCamera` |
| Dialogue | The message box and typing | `DialogueBox`, `SpeechPlayback`, `Typewriter`, `FoundItemText` |
| Title | The title screen | `TitleScene`, `TitleMenu` |
| Battle | The battle screen over the map: turns, commands, playback (ADR 0013) | `BattleScene`, `BattleTurnSequence`, `TurnStage`, `BattlePacing`, `ActionPlayback`, `PlaybackBeat`, `BattleBanner`, `BattleBannerView`, `FloatingNumber`, `FloatingNumbersView`, `EffectStrip`, `PlaceholderCue`, `ImpactEffectsView`, `BattleLayout`, `BattlerArt`, `BattlerScaling`, `BattlersView`, `VitalsText`, `PartyStatusView`, `BattleVitalRow`, `EnemyLabels`, `EnemyTagView`, `TurnOrderStrip`, `TurnOrderView`, `TurnDiamondView`, `VictoryScreen`, `VictoryScreenView`, `VictoryEntrance`, `EscapedNotice`, `DefeatScreen`, `DefeatScreenView`, `DefeatEntrance`, `DefeatMenu`, `DefeatEntry`, `BattleSceneSources`, `LevelUpPop`, `VictoryRewardRow`, `ExperienceBarAnimation`, `CommandMenu`, `CommandEntry`, `BattleSkillChoice`, `BattleItemChoice`, `IBattleListChoice`, `CommandRowLabel`, `BattleTargetCursor`, `CommandPanelView`, `CommandPanelPlacement`, `PointerBob` |
| Menus | The game menu and its pages | `GameMenuScene`, `IMenuPage`, `StatusPage`, `InventoryPage`, `EquipmentPage`, `EquipmentSheet`/`EquipmentSheetView`, `EquipmentChoice`/`EquipmentChoiceView`, `StatPreviewText`, `PartyPage`, `PartyCursor`, `PartyColumns`/`PartyColumnsView`, `PartyActionChoiceView`, `OptionsPage`, `SystemPage` |
| Saving | The 20 save files and their list | `SaveSlots`, `SaveSlotList`, `SaveFilesPanel` |
| Ui | Shared UI pieces: theme, text, cursors, panels, bars, shapes | `UiTheme`, `UiTextures`, `UiMessages`/`MessageIds`, `ListCursor`, `StatBar`, `SectionTitle`, `MeasuredText`, `VitalMeter`, `MemberVitalBars`, `MemberCardView`, `Diamond`, `ConfirmPrompt`, `Vignette`, `OutlinedFont`, `DiamondShape`, `OutlinedShape`, `CommandHeaderShape`, `PointedBarShape` |

## Jrpg.Editor (WPF tools)

| Folder | Purpose | Key types |
|---|---|---|
| Shell | The main window, its menu bar and editing modes; F1 lists the controls (a new gesture adds its line there) | `MainWindow`, `MainViewModel`, `EditingMode`, `MapEntry`, `ControlsHelpWindow` |
| Maps / Maps/Editing / Maps/Settings | The map document, its undoable edits, map settings and new maps | `MapViewModel`, `MapDraft`, `IMapEdit`, `EditHistory`, `MapSettingsViewModel`, `LayerChoiceViewModel`, `LayerEntry` |
| Catalogs | The content the editor works on, read by every panel and the drawing; catalog changes go through it (ADR 0014) | `CatalogDraft`, `SheetCell` |
| Tiles, Objects | Tile and object browsers, painting and placing | `TilePaintingViewModel`, `ObjectPlacingViewModel` |
| Events | Object and cell event dialogs and presets | `EventEditingViewModel`, `EventStepViewModel`, `EventPresets` |
| Viewport | Zoom and pan | `ViewportTransform` |

## Where to look

- A rule or formula → Core, its folder above; formulas sit in one static class per domain (`DamageFormula`, `ExperienceTable`).
- A content field → the record in Core, its validator in Data, the JSON in `content/`.
- A screen → `Jrpg.Game/<Feature>`; its tests in `tests/Jrpg.Game.Tests/<Feature>`.
- Text the player reads → `content/text/ui.json` and `Ui/MessageIds`.
- A design decision → `docs/decisions/README.md`; a feature's plan → `docs/plans/README.md`.
