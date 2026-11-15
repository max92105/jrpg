# CG2. The game plays a cinematic step

Game, about 250 lines (about 120 of them tests). Needs CG0, CG1, CM3. Decisions 628, 629; design "Structure" (Game),
risk "Loading needs a draw".

## Goal
A `cinematic` step keeps the screen black while its stills load, one per frame in order (decision 628); then the black
lead-in counts; then each still of the record shows for its seconds, faded in and out by Core's fade, with its subtitle
under it; then the black lead-out before what follows (a warp after it starts already black). Known gap until CG4: a
cinematic can't be skipped (a single press does nothing, which is already the final rule); voices and the track come in
CG3.

## New types
- Game `Exploration/StillPreload` (internal sealed): the stills a cinematic still needs loaded, one asked per frame, in
  order. Built from the cinematic's stills; `string? Next` (the picture to load this frame; null once done),
  `MarkLoaded()`, `IsDone`. Pure, no texture.
- Game `Exploration/CinematicRun` (internal sealed): one cinematic step on screen: loading, black before, its stills by
  their time, black after (the hold to skip joins it in CG4).
  - `LeadIn`, `LeadOut` (0.5 s, the same values as `StillRun`'s, which CR3 deletes).
  - `void Show(CinematicDefinition cinematic, TimeSpan delta)`: the first update builds the `StillPreload`; while it
    isn't done, nothing counts; then the lead-in counts down; then the cinematic's elapsed time grows by the delta and
    `MomentAt` gives the still; a new still index measures its subtitle once (`StillView.Begin`); past the end,
    `ExplorationState.ContinueEvent()` and the lead-out start.
  - `IsShowing`, `IsRunning`, `IsLeadingOut`, `AdvanceLeadOut(delta)`, `Blackness` (1 while loading and in the lead-in
    and lead-out).
  - `void Draw(SpriteBatch batch, TextureLibrary textures)`: while loading, `textures.Get(ArtFolders.Cinematics,
    preload.Next)` then `MarkLoaded()`, one per frame, drawing nothing (the draw may touch the device, the update never
    does, ADR 0008); then the shown still through `StillView` at `StillFade.OpacityAt`.
  - `internal void MarkStillsLoaded()` (the one draw-side state a headless test can't produce) and `internal
    CinematicStill? ShownStill` (the test seams).

## Changes
- Game `Exploration/StillView`: `Begin(string? caption)` and `Draw(SpriteBatch, TextureLibrary, string imageFile, float
  opacity)` instead of taking a `StillStep`/`StillPlayback`; `StillRun` passes `still.Step.ImageFile` and
  `still.Opacity`. The skip hint stays as today for both runs until CG4.
- Game `Exploration/CutscenePresenter`: a `CinematicRun` beside the `StillRun`; `Show(CinematicStep, ...)` reads
  `exploration.CurrentCinematic!`; `IsShowingStill`, `IsLeadingOut`, `Blackness` and `Draw` cover both runs.
- Game `Exploration/EventPresenter`: `case CinematicStep:` beside `case StillStep` (the same "stay black into a warp"
  after it); `internal string? ShownStillImage` and `internal void MarkStillsLoaded()` forwarding to the cutscene. Its
  summary says a cinematic step plays like the stills.
- `tests/Jrpg.Game.Tests/Exploration/EventPresenters.cs`: `PastLoading(presenter)` (calls `MarkStillsLoaded`).
- `docs/map.md`: Game `Exploration` names `CinematicRun`, `StillPreload`.

## Tests
- `StillPreloadTests` (new): `Next_AtStart_IsTheFirstPicture`; `MarkLoaded_EachInTurn_AsksTheNextInOrder`;
  `IsDone_AfterTheLast_IsTrueAndNextIsNull`.
- Game `EventPresenterCinematicTests` (new), on `StartMapScenarios.BelowChestPlayingCinematic` (new scenario: the chest
  says `[cinematic test_cinematic, dialogue]`, the catalog `with` `CinematicFixtures.TwoStills` from
  `tests/Jrpg.Game.Tests/Exploration/CinematicFixtures.cs`: `a.png` 4 s then `b.png` 4 s, captions "Long ago." and "Far
  away."), stepping at `StartMapScenarios.OneUpdate` through the fades:
  - `Update_StillsNotLoaded_LeadInDoesNotCount` (many updates, still black, no still shown).
  - `Update_PastLoadingAndTheLeadIn_ShowsTheFirstStill` (`ShownStillImage` is `a.png`).
  - `Update_PastTheFirstStillsSeconds_ShowsTheSecond` (`b.png`; blackness 0: no black hold between stills).
  - `Update_ConfirmPressedDuringAStill_DoesNothing` (decision 623's single press; the still keeps showing).
  - `Update_LastStillOver_HoldsBlackForTheLeadOutBeforeTheDialogue`.
  - `Update_CinematicThenWarp_WarpStartsAlreadyBlack` (a scenario with `[cinematic, warp]`: the map never shows between
    them, the tests rule's "through to the step that follows").
- `StartMapScenarios` (202 → about 212): the two scenarios.

## Verify by hand
In the editor, Events mode on map **intro**, click cell (0, 1) (Ragnir's intro): click **Cinematic**, choose
`ragnir_intro`, move it to the top with **Up**; OK; **Ctrl+S**. Run the game, New game, choose Ragnir: black (a moment
longer while the six stills load), then the six stills in order, each fading in and out for its seconds with its
subtitle, no stutter at a still's start; then the old placeholder stills (still in the event until CG3). Revert the
intro map afterwards (`git checkout content/map-layouts/intro.json`): CG3 switches it for good.

- **Note from CM3:** `ExplorationState.CurrentCinematic` (the running event's `EventRun.CurrentCinematic`) has no test of its own yet: this slice's tests read it through the presenter.

- **Done, with deviations:** `CutscenePresenter.Show` takes the step and picks the run, so `EventPresenter`'s switch gains no case (`StillStep or CinematicStep`). The tests call `MarkStillsLoaded` on the presenter directly; no `EventPresenters.PastLoading` helper. The skip hint still shows during a cinematic though a press does nothing, until CG4 replaces it. `EventPresenter.cs` is at 281 lines.
  CG2's review: `EventPresenter.ShownStillImage` is `ShownCinematicImage`, as the cutscene presenter names it (it reads the cinematic's picture only). The scenarios build their cinematic in `StartMapScenarios.WithTestCinematic`, not a `CinematicFixtures` file. `CinematicRun` has no `IsRunning`: `CutscenePresenter.Show` returns true for a cinematic, which nothing skips until CG4.
