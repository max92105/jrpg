# Cinematics in the editor: design

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Still fade | `Core/Cinematics/StillFade.FadeTime` (moved from `Game/Exploration/StillPlayback.ShotFade` in CG1) | 0.8 s, at most half the still's time | How long a still takes to fade in from black, and again to fade out. |
| Lead-in, lead-out | `Game/Exploration/CinematicRun.LeadIn`, `LeadOut` (today `StillRun`'s) | 0.5 s each | Black before the first still (counted once the stills are loaded) and after the last. |
| Hold to skip | `Game/Exploration/SkipHold.HoldTime` | 2 s (decisions 623, 624) | How long Confirm is held to skip a cinematic. |
| Ring | `Game/Ui/ProgressRingView` | 48 segments, about 40 px across | The hold's ring around the key's name (decision 626). |
| Zoom bounds | `Core/Cinematics/StillZoom.MinPercent`, `MaxPercent` | 100, 150 | A zoom percent's range (decision 617). |
| New still | `Editor/Cinematics/CinematicStillRow.New` | 4 s, no zoom (decision 616) | The boxes of a still just added. Editor only. |
| Preview size | `Editor/Cinematics/CinematicCatalogWindow.xaml` | 480 x 270 | The preview's area: the game's 1920 x 1080 screen at a quarter. |

## Design overview

### What exists
- **Core** `Events/StillStep(ImageFile, Seconds, Caption)` (presentation, waits until shown) and `Events/VoiceStep(File)`
  (presentation, applied at once: `EventRun.StartedVoice`, read through `ExplorationState.StartedVoice`, cleared by
  `SkipPresentation`). `EventRun.ApplyAndCheckWait` puts `StillStep` in the `or` list of steps that wait; `EventRun`
  holds `ExplorationContext.Catalog`, so it can resolve an id (as the give-item step reads `Catalog.Items`).
- **Content** `content/map-layouts/intro.json`: nine `arrive` cell events, each gated `onlyWhenLeaderId`, each `voice
  Placeholder.wav`, three `still` steps (`Placeholder1..3.png`, 4 s, a placeholder caption) and a `warp` to the start
  map (Ragnir's at cell (0, 1) warps to `milestone` (23, 24); Omer's at (1, 1)). `new-game.json` starts every leader on
  `intro` (2.5, 2.5). `content/art/cinematics/` holds the three placeholders and the new, untracked `ragnir_intro/`
  folder (six PNGs of about 2.7 MB). The intro map has no track; `milestone` plays `town/loop.wav`. `rules.json` holds
  game rules (party, stats, exploration, experience, loot, trade, crafting), no UI timing.
- **Data** `Maps/MapEventValidator.FindStillProblem` (image named, seconds above 0 and countable, caption null or text),
  the voice step's file named; `Art/ArtReferenceCheck` checks every still by exact name (`ExactFileName` matches names
  under subfolders, so `ragnir_intro/1_happy_life.png` already works); `Audio/AudioReferenceCheck` every voice step's
  file; `Audio/AudioFolders.FilesIn` and `TracksIn` list voices and tracks; `Art/ArtFolders.ImagesIn` lists a folder's
  PNGs, top level only.
- **Game** `Exploration/EventPresenter` (294 lines) owns a `StillRun` (lead-in, each `StillPlayback`, lead-out, a press
  of Confirm or Cancel skipping through `SkipEventPresentation`), the voice following (`FollowStartedVoice` writes the
  `VoiceChannel` from Core's `StartedVoice`), and `StayBlackIntoAWarp`. `StillView` draws a `StillPlayback`: black, the
  picture through `Ui/CoverFit.DrawOverScreen` (an integer source rectangle, the scene's batch), the subtitle band, the
  skip hint (`cinematic_skip`: "{button} Skip"). `Input/InputFrame` has `IsPressed` and `IsHeld`. `Flow/MusicSelection`
  picks the map's track unless a battle calls for its own. `Ui/UiTextures` bakes shapes once (`DiamondShape`,
  `PointedBarShape`, `TabGlyphShape`).
- **Editor** `Events/EventStepViewModel` (272 lines; `Label`, `Problem`, `From`, `ToStep` switch over every kind,
  `ToStep` already split by `QuestStep()` for its complexity), `Events/EventStepKind`, the step editor and the two event
  windows' Add buttons. The Database windows (`Trade/`, `Battle/`, `Loot/`, `Quests/`) are the pattern: a row view model
  per typed record, a record view model, a catalog view model, an editing view model (`Start`/`Apply`), the window;
  `Catalogs/RecordListProblem`, `Catalogs/TypedNumber`, `Events/ListOrder`, `Objects/ObjectIds`.

### Core changes
- **`Cinematics/` (new leaf folder).** CM1: the records. CE1: `CinematicDefinition` equal by content (as
  `ShopDefinition`). CG1: `StillFade` (the 0.8 s moved from Game), `StillMoment`, `CinematicDefinition.Duration` and
  `MomentAt`. CG3: `CinematicDefinition.VoicedStillAt` (decision 613). CG6: `StillZoom.ScaleAt`,
  `CinematicStill.ScaleAt`.
- **`Content/ContentCatalog.Cinematics`** (CM1): by id, an init property defaulting to empty (as `FieldSprites`), so no
  test catalog built by hand breaks.
- **`Events/CinematicStep(string CinematicId)`** (CM3): `IsPresentation` true, so a skip runs past it and still applies
  the warp after it (ADR 0016).
- **`Exploration/EventRun.CurrentCinematic`** and **`ExplorationState.CurrentCinematic`** (CM3): the record the current
  step plays, resolved from the catalog; null on any other step. The step joins the `or` list of steps that wait.
- CR3 deletes `Events/StillStep`; `VoiceStep` and `StartedVoice` stay (decision 621).

### Content format
`content/cinematics.json` (CM1 creates it empty; the developer fills it in the window), as the writer lays it out (CE1),
sorted by id, the stills in their order, optional fields left out while null:

```json
[
  {
    "id": "ragnir_intro",
    "musicFile": "town/loop.wav",
    "stills": [
      {
        "imageFile": "ragnir_intro/1_happy_life.png",
        "seconds": 6,
        "caption": "We were happy, once.",
        "voiceFile": "ragnir_intro_1.wav",
        "zoom": {
          "startPercent": 100,
          "endPercent": 108
        }
      }
    ]
  }
]
```

An event plays it with `{ "type": "cinematic", "cinematicId": "ragnir_intro" }`.

### Structure and folder dependencies
Core:

| Type | Responsibility |
|---|---|
| `Cinematics/CinematicDefinition` (CM1, CE1, CG1, CG3) | One cinematic as content: its stills in order and the track under them. |
| `Cinematics/CinematicStill` (CM1, CG6) | One still as content: its picture, time, subtitle, voice line and zoom. |
| `Cinematics/StillZoom` (CM1, CG6) | A still's slow zoom: its scale at the start and at the end of its time. |
| `Cinematics/StillFade` (CG1) | A still's opacity at a time into it: in from black, holding, out to black. |
| `Cinematics/StillMoment` (CG1) | Which still a cinematic shows at a time, and how far into it. |
| `Events/CinematicStep` (CM3) | An event step that plays a cinematic by id and waits until the Game has shown it. |

| From | To | Why |
|---|---|---|
| `Cinematics` | (none) | A leaf: records and their time rules. |
| `Content` | `Cinematics` | `ContentCatalog.Cinematics`. |
| `Exploration` | `Cinematics` | `EventRun.CurrentCinematic`. |
| `Events` | (nothing new) | `CinematicStep` holds an id only. |

Data: `Cinematics/CinematicCatalogValidator` (CM1; checks `cinematics.json`), `Content/ContentWriter.Catalogs.cs`
(CE0; the catalog list, moved), `Audio/WavDuration` (CE7; a WAV's length from its header). `Content` → `Cinematics` (the
loader and `CatalogProblems` call the validator), as for every other catalog.

Game:

| Type | Responsibility |
|---|---|
| `Exploration/CutscenePresenter` (CG0) | Shows the running event's cutscene: its stills or cinematic, the black around them and the voice it asks for. |
| `Exploration/StillPreload` (CG2) | The stills a cinematic still needs loaded, one asked per frame, in order. |
| `Exploration/CinematicRun` (CG2-CG6) | One cinematic step on screen: loading, black before, its stills by their time, black after, the hold to skip. |
| `Exploration/SkipHold` (CG4) | How far the hold to skip has come: grows while Confirm is held, back to nothing on release, done at 2 s. |
| `Ui/RingSegmentShape` (CG5) | The ring's one segment as a small mask, baked once by `UiTextures`. |
| `Ui/ProgressRingView` (CG5) | Draws a ring of segments about a center, lit up to a share. |

`StillView` draws a still from what it shows (picture, opacity; scale and its own linear batch in CG6) rather than from
a `StillPlayback` (CG2), and the hint with the ring (CG4, CG5). `StillRun` and `StillPlayback` stay for `StillStep`
until CR3 deletes them. `Flow/MusicSelection` reads `ExplorationState.CurrentCinematic` (CG3). Edges: `Exploration` →
`Ui` exists; `Ui` gains no feature edge (the ring is generic).

Editor (new folder `Cinematics/`, namespace `Jrpg.Editor.Cinematics`):

| Type | Responsibility |
|---|---|
| `CinematicStillRow` (CE3, CE7) | One still as typed: picture, seconds, subtitle, voice and zoom boxes, why a box can't be read, and its voice's length. |
| `CinematicViewModel` (CE4) | One cinematic as edited: its id (typed while new), its track and its still rows in order. |
| `CinematicMediaChoices` (CE5, CE7) | The pictures, voices and tracks the boxes offer and a voice's length, read through lookups. |
| `CinematicCatalogViewModel` (CE5, CP1) | Every cinematic as the window edits it, and the window's problem line. |
| `CinematicEditingViewModel` (CE5) | Starts the window's state from the draft and applies it as one undo step. |
| `CinematicCatalogWindow` (CE6, CE7, CP1) | The window: cinematics left, the chosen one's track and stills right, the preview. |
| `CinematicPreview` (CP1) | The preview's moment on its own clock: the still shown, its opacity, scale and subtitle. |

| From | To | Why |
|---|---|---|
| `Cinematics` | `Catalogs` | `CatalogDraft`, `TypedNumber`, `RecordListProblem`. |
| `Cinematics` | `Events` | `ListOrder` (moving rows). |
| `Cinematics` | `Objects` | `ObjectIds` (the id rule). |
| `Shell` | `Cinematics` | The menu entry and the editing view model. |
| `Events` | (nothing new) | `StepChoices.CinematicIds` reads the draft's catalog only. |

### Key decisions
Decisions 609-630 in the README; the ones that shape the code: 609 (record + step), 613 and 622 (whose voice plays),
623-626 (the hold and its ring), 627 (linear sampling for every still), 628 (loading before the lead-in), 629 (time and
look rules in Core), 630 (the undo refusal).

### Extension points
- A pan toward a point (decision 618) is a field beside `Zoom` on `CinematicStill` and a translation in the draw.
- Another way to start a cinematic (the title, a battle's end) would read the catalog's record the same way; none is
  planned.

### Replacement mapping

| Existing | Replaced by | When |
|---|---|---|
| `voice` + `still` runs in `intro.json` | a `voice` step then one `cinematic` step per intro | CG3 (Ragnir, voice step removed), CR1 (the eight others, voice step kept) |
| `Game/Exploration/StillRun` | `CinematicRun` | CG2 (beside it), CR3 (deleted) |
| `StillPlayback.Opacity`, `ShotFade` | `StillFade.OpacityAt`, `FadeTime` | CG1 (delegates), CR3 (deleted) |
| `EventPresenter`'s still and voice handling | `CutscenePresenter` | CG0 |
| Press Confirm or Cancel to skip | hold Confirm 2 s (cinematics only) | CG4 |
| `MapEventValidator.FindStillProblem` | `CinematicCatalogValidator` | CR3 |
| The event dialog's Still kind | the Cinematic kind and the Cinematics window | CR3 |

## Risks
- **Two ways to show stills until CR3:** `StillRun` (press to skip) and `CinematicRun` (hold to skip) coexist for about
  twelve slices; each slice's hand check names which one it touches.
- **Timing drift:** voices play on the audio device's clock, stills on the update's (ADR 0016 accepts it); a voice
  longer than its still runs over the next one by design (decision 613). CE7's length line is the tool to time them.
- **Loading needs a draw:** `StillPreload` is advanced by `Draw`, which headless tests never call; CG2 adds one test seam
  (`EventPresenter.MarkStillsLoaded`, used by the test helper only) and tests `StillPreload` on its own.
- **Memory:** six painted stills (sizes unread) stay in `TextureLibrary` after the intro (decision 619).
- **Sampler state:** CG6 draws stills in their own linear batch; the batch must be ended and the scene's begun again in
  the presenter's draw order (picture, then subtitle, hint and ring, then the fade).
- **Complexity limits:** `EventRun.ApplyAndCheckWait` (the waiting `or` list), `MapEventValidator.FindStepProblem` and
  `EventStepViewModel.ToStep` each gain an arm in CM3; CM3 names the helper each one moves into.
- **The test world:** its frozen `intro.json` keeps `still` steps until CR2; its catalogs are the live ones, so CR2
  freezes a test-world `cinematics.json`.
- **Editor decoding:** the window decodes 2.7 MB stills on the UI thread; CE6 decodes at the preview's width and caches
  per file while the window is open.
- **Determinism:** none at risk: no randomness, every time from deltas. **Saves:** unchanged.
