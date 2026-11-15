# CR3. The `still` step leaves

Core + Data + Game + Editor, about 60 lines added and about 400 deleted. Needs CR2. Decisions 621, 625; design
"Replacement mapping".

## Goal
One way to show stills: `StillStep` is gone from Core, Data, the game and the editor, with its tests; an old content file
holding one is refused by the loader as an unknown step type (no content holds one after CR1). The `voice` step, Core's
`StartedVoice` and the game's following of it stay (decision 621).

## Changes
Each by the "Replacement mapping" table; grep each removed name across `src/` and `tests/` (type-scoped: `StillStep`,
`StillRun`, `StillPlayback`, `IsStill`, `AddStill`, `FindStillProblem`, `ShotFade`, `IsShowingStill` where it means the
still run) before and after.
- Core: `Events/StillStep.cs` deleted; `EventRun`'s waiting `or` list loses it.
- Data: `ContentTypeDiscriminators`' `still` line; `MapEventValidator.FindPresentationProblem` keeps the voice's and the
  cinematic's rules; `ArtReferenceCheck`'s still-step line (the summary's "every cutscene still" becomes "every
  cinematic's still").
- Game: `StillRun.cs`, `StillPlayback.cs` deleted (`CinematicRun` owns the lead-in and lead-out values); `StillView`
  loses the still step's press hint (`cinematic_skip` leaves `ui.json` and `MessageIds` if nothing else reads it);
  `CutscenePresenter` plays only cinematics and keeps writing the voice (`StartedVoice` or the cinematic's);
  `EventPresenter`'s `case StillStep`.
- Editor: `EventStepKind.Still`; `EventStepViewModel`'s still fields (`ImageFile`, `SecondsText`, `Caption`) and arms;
  `PresentationStep()` keeps the voice and the cinematic; the step editor's still row; the two event windows' Still
  button; `EventViewModel.AddStill`.
- Tests deleted with their code: `StillPlaybackTests`, `MapEventValidatorTests`' still rows, `EventStepViewModelTests`'
  still cases, the still-step case of `ArtReferenceCheckTests`; `StillFadeTests` stays (`CinematicRun` reads
  `StillFade`).
- `docs/map.md`: the Core `Events` row loses `StillStep`; the Game `Exploration` row loses `StillRun`, `StillPlayback`;
  ADR 0016's line in `docs/decisions/README.md` points to ADR 0046.

## Tests
- `ContentCatalogLoaderCinematicTests`: `Load_LayoutWithAStillStep_IsRefusedAsUnknownType` (message pinned: the
  serializer's unknown discriminator, naming the file).
- Runner totals per project before and after; the deleted test names listed in the slice's notes.

## Verify by hand
1. New game with Ragnir, then with Sarxian: both intros play as after CR1 (Sarxian's under the placeholder voice).
2. In the editor, Events mode on map **intro**, click cell (1, 1) (Omer's intro): the step list shows "Voice:
   Placeholder.wav", "Cinematic: omer_intro" and "Warp to: ..."; the Add buttons offer Voice and Cinematic, no Still.

- **Done, with deviations:** `MapFolderReaderTests.Read_ArrivalEventWithStill_LoadsTheStillAndTheActivation` is deleted with the step it read (reading an arrive event stays covered by the repository's intros). The skip hint keeps only the hold's text: `cinematic_skip` leaves `ui.json` and `MessageIds`, and `SkipHintView.Follow` takes the device alone. `CutscenePresenter.Show` takes the time and input alone (only a cinematic step plays through it). Deleted tests: `StillPlaybackTests` (3), `ArtReferenceCheckTests.FindProblems_MissingStill_NamesTheEventAndMap`, `MapEventValidatorTests`' five still rows, `EventStepViewModelTests`' still label, seconds (5 rows) and caption cases and two still rows of `StepsOfEveryKind`, `EventStepViewModelCinematicTests.ToStep_Still_KeepsItsFieldsAfterTheMove`, `MapFolderReaderTests`' still read; new: `ContentCatalogLoaderCinematicTests.Load_LayoutWithAStillStep_IsRefusedAsUnknownType`. Totals: Core 1303 → 1303, Data 896 → 890, Game 1472 → 1469, Editor 1549 → 1539.
  CR3's review: two comments (`EventPresenter`, `StillView`) name the cinematic and ADR 0046; `CoverFit`'s two cover draws are a backlog line.
