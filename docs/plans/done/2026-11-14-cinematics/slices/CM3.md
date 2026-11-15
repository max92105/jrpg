# CM3. A `cinematic` event step

Core + Data + Editor, about 250 lines (about 130 of them tests). Needs CM1. Design: "Core changes", key decision 1,
risk "Complexity limits".

## Goal
A map event can hold `{ "type": "cinematic", "cinematicId": "..." }`: Core waits on it with the record resolved, Data
refuses an unknown id, the event dialog adds, shows and edits it. The game doesn't play it yet (CG2): no content uses
it until CG3, and `EventPresenter` throws on a step it can't show, so the known gap is safe.

## New types
- Core `Events/CinematicStep(string CinematicId)` (public sealed record, `EventStep`): plays a cinematic by id and waits
  until the Game has shown it (ADR 0046). `IsPresentation => true`: a skip runs past it and still applies the steps
  after it (ADR 0016).

## Changes
- Core `Exploration/EventRun` (208 → about 216): `CinematicStep` joins the `or` list of steps that wait
  (`DialogueStep or WarpStep or StillStep or CinematicStep or ...`); if CA1502 fires on `ApplyAndCheckWait`, that `or`
  list moves into a static `IsShownStep(EventStep step)` the case calls. `public CinematicDefinition? CurrentCinematic =>
  Current is CinematicStep step ? _context.Catalog.Cinematics[step.CinematicId] : null;` (summary: the record the
  current step plays; Data checks the id, so a missing one is a content bug and throws).
- Core `Exploration/ExplorationState` (271 → about 274): `CurrentCinematic => _events.Run?.CurrentCinematic`, beside
  `StartedVoice`.
- Data `Content/ContentTypeDiscriminators`: `new JsonDerivedType(typeof(CinematicStep), "cinematic")`.
- Data `Maps/MapContentReferences`: `IReadOnlySet<string> CinematicIds { get; init; }` (empty by default, as
  `ShopIds`); `ContentCatalogLoader` and `ContentWriter.ContentReferencesOf` (294 → 295) set it.
- Data `Maps/MapEventValidator` (243 → about 250): the `StillStep` and `VoiceStep` arms of `FindStepProblem` become one
  `StillStep or VoiceStep or CinematicStep => FindPresentationProblem(step, references)` arm (the switch shrinks by
  one); `FindPresentationProblem` holds the still's and voice's rules as they are and `cinematic has unknown cinematic id
  'x'.` The class summary is unchanged (it names no step).
- Editor `Events/EventStepKind`: `Cinematic`.
- Editor `Events/EventStepViewModel` (272 → about 284): `IsCinematic`; `[ObservableProperty]` `CinematicId` (Label,
  Problem) with summary "The cinematic the step plays, picked from the draft's (ADR 0046)."; `Label` "Cinematic:
  ragnir_intro"; `Problem` "Choose the cinematic." while empty; `From`; `ToStep`'s `Still` and `Voice` arms move into a
  `PresentationStep()` helper beside `QuestStep()`, with the cinematic (`string.IsNullOrEmpty(CinematicId) ? throw
  InvalidFields() : new CinematicStep(CinematicId)`), so `ToStep` stays under the complexity limit.
- Editor `Events/StepChoices`: `IReadOnlyList<string> CinematicIds { get; init; } = []`; `EventEditingViewModel` fills it
  from `catalog.Current.Cinematics.Keys`, sorted ordinally, as `ShopIds`.
- Editor `Events/EventViewModel`: `AddCinematic` command (`AddStep(EventStepKind.Cinematic)`).
- Editor XAML: `EventStepEditor.xaml` a "Cinematic" row (a combo over `Choices.CinematicIds`, as the shop's);
  `ObjectEventsWindow.xaml` and `CellEventWindow.xaml` a "Cinematic" button beside "Still" and "Voice".
- `Shell/ControlsHelpWindow.xaml` needs nothing (no gesture).
- `docs/map.md`: the Core `Events` row names `CinematicStep`; `Exploration` names `CurrentCinematic`; the Editor
  `Events` row names the cinematic step.

## Tests
- `EventRunCinematicTests` (new sibling; `EventRunTests` is at 269), on a catalog with one two-still cinematic
  `test_cinematic` (`CinematicFixtures` in `tests/Jrpg.Core.Tests/Cinematics/`, built `with` over the Core tests'
  catalog) and an event of `[cinematic, giveItem potion, dialogue]`:
  - `Start_CinematicStep_WaitsWithItsRecord` (`Current` is the step, `CurrentCinematic.Id` is `test_cinematic`).
  - `Continue_PastCinematic_ClearsCurrentCinematic` (the give item applied, the dialogue current, `CurrentCinematic`
    null).
  - `SkipPresentation_OnCinematic_AppliesTheGiveItemAndStopsAtTheDialogue`.
- `MapEventValidatorTests` (204): `InvalidLayouts`-style rows for an unknown cinematic id (message pinned: `event
  'e': step 0: cinematic has unknown cinematic id 'nope'.`); `FindProblem_KnownCinematic_IsNull` (the accepted side).
- `ContentCatalogLoaderCinematicTests` (CM1's): `Load_CinematicStep_ReadsItsId`.
- `ContentWriterCinematicTests` (new; `ContentWriterTests` is at 254): `WriteMaps_CinematicStep_WritesTypeAndId` (the
  layout file holds `"type": "cinematic"` and the id; loads back equal).
- `EventStepViewModelCinematicTests` (new sibling; `EventStepViewModelTests` is at 266):
  - `From_CinematicStep_KeepsItsId`.
  - `ToStep_ChosenCinematic_MakesTheStep`.
  - `Problem_NoCinematicChosen_AsksForOne`, `Problem_CinematicChosen_IsNull`.
  - `Label_Cinematic_NamesIt`.
  - `ToStep_StillAndVoice_StillMadeAfterTheMove` (the moved arms: a still with its caption, a voice with its file).
- `EventEditingViewModelTests` (or its sibling past 250): `Start_DraftCinematics_OfferedSorted` (`zeta`, `alpha` in the
  draft: `alpha` first).

## Verify by hand
1. Run the editor. Click map **intro** in the map list, switch to **Events** mode, and click cell (0, 1) (the second
   cell of the left column, Ragnir's intro; Ctrl + click if an object covers it): the cell's event dialog opens.
2. Click **Cinematic** under the steps: a "Cinematic:" step appears with an empty box and the problem "Choose the
   cinematic."; the list is empty (no cinematic yet). Click **Cancel**: nothing changes.

- **Done, with deviations:** CA1502 didn't fire, so `MapEventValidator` gains one `CinematicStep when` arm and its still and voice arms stay as they are (merging them would refactor code the slice doesn't need). No separate `Load_CinematicStep_ReadsItsId`: `ContentWriterCinematicTests.WriteMaps_CinematicStep_LoadsBackEqual` reads the step back through the loader. The skip test stops at the give item, which waits, not at the dialogue. The choices test is a new sibling, `EventEditingViewModelCinematicTests` (`EventEditingViewModelTests` is at 286). `ExplorationState.CurrentCinematic` is a pass-through tested by its first reader, CG2.
