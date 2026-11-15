# CG3. A cinematic's voices and track; Ragnir's intro plays its cinematic

Core + Game + content, about 260 lines (about 150 of them tests). Needs CG2, and `ragnir_intro` in
`content/cinematics.json` (the developer's, CE6). Decisions 613, 614, 622.

## Goal
Each still's voice line starts with its still and plays on until another still's voice starts or the cinematic ends
(decision 613); the cinematic's track plays from the step's first update to its last still, then the map's (decision
614). A voice an earlier `voice` step started is cut by the cinematic's track at its start, or else by its first voiced
still, and plays on otherwise; once cut it doesn't come back in that event (decision 622). Ragnir's intro event becomes
`[cinematic ragnir_intro, warp milestone (23, 24)]`.

## Changes
- Core `Cinematics/CinematicDefinition`: `int? VoicedStillAt(int stillIndex)`: the still whose voice speaks while this
  one shows (itself when it has a voice, else the latest earlier one with a voice; null when none has), scanning back
  without allocating. Summary: decision 613's rule.
- Game `Exploration/CinematicRun`: `string? Voice` (the voiced still's `VoiceFile` at the shown still; null while
  loading, in the lead-in, after the last still) and `bool CutsEarlierVoice` (true from the step's first update when the
  cinematic has a track, else from its first voiced still).
- Game `Exploration/CutscenePresenter`: the voice it writes: the cinematic's `Voice` when set; else Core's
  `StartedVoice` unless a cinematic of this event cut it (a flag set when `CutsEarlierVoice` turns true, cleared when
  `StartedVoice` changes to another step, so a later `voice` step plays); else `Stop()`.
- Game `Flow/MusicSelection.TrackFor`: on the field, `session.Exploration.CurrentCinematic?.MusicFile ??` the map's
  track; the summary names the cinematic (ADR 0035's "chosen from state": the step is Core state).
- `content/map-layouts/intro.json`: Ragnir's cell event (0, 1): its `voice` step and three `still` steps become one
  `{ "type": "cinematic", "cinematicId": "ragnir_intro" }`, as the writer lays it out (or in the editor: Events mode,
  cell (0, 1), remove the voice and the three stills, add Cinematic `ragnir_intro` at the top, OK, Ctrl+S).
- `tests/Jrpg.Data.Tests/Session/RepositoryNewGameTests`: `StartNew_RepositoryContent_EveryIntroStartsWithTheVoice`
  becomes `..._EveryIntroPlaysAVoiceOrACinematic` (each intro holds a `VoiceStep` or a `CinematicStep` whose id the
  catalog has); the Zanyr test is untouched (its stills stay until CR1).

## Tests
- Core `CinematicDefinitionTests`: `VoicedStillAt_StillWithVoice_IsItself`; `VoicedStillAt_StillWithout_IsTheLatestEarlierVoiced`
  (stills voiced, voiced, unvoiced: index 2 gives 1, not 0); `VoicedStillAt_NoEarlierVoice_IsNull`.
- `EventPresenterCinematicTests` (CG2's), stills `a.png` with `one.wav`, `b.png` with none, `c.png` with `three.wav`
  (`CinematicFixtures.ThreeStillsWithVoices`), past loading:
  - `Update_LeadIn_AsksNoVoice`.
  - `Update_FirstStillShown_AsksItsVoice` (the channel's `VoiceFile` is `one.wav`).
  - `Update_StillWithoutVoice_KeepsTheEarlierOne` (`one.wav` on `b.png`).
  - `Update_NextStillsVoice_ReplacesIt` (`three.wav`).
  - `Update_LeadOut_SilencesTheVoice`.
- `EventPresenterCinematicVoiceTests` (new sibling: the file above passes 250), the chest saying `[voice old.wav,
  cinematic, dialogue]`:
  - `Update_CinematicWithTrack_CutsTheEarlierVoiceAtItsStart`.
  - `Update_CinematicWithoutTrackOrVoices_EarlierVoicePlaysOn` (through the last still).
  - `Update_CinematicWithoutTrack_EarlierVoicePlaysUntilTheFirstVoicedStill` (on `a.png`, `one.wav` replaces it).
  - `Update_AfterTheCinematicCutIt_EarlierVoiceDoesNotComeBack` (the dialogue after: silent, not `old.wav`).
  - `Update_VoiceStepAfterTheCinematic_Plays` (`[voice old.wav, cinematic, voice new.wav, dialogue]`: `new.wav`).
- `MusicSelectionTests` (86):
  - `TrackFor_CinematicWithTrack_IsItsTrack`.
  - `TrackFor_CinematicWithoutTrack_IsTheMapsTrack` (a map with a track: the filter's other side).
  - `TrackFor_AfterTheCinematicStep_IsTheMapsTrack`.
- `RepositoryNewGameTests`: the re-pinned test above.

## Verify by hand
1. If `ragnir_intro` names voices or a track, check they are in `content/audio/` (the game's start names a missing one).
2. Run the game, New game, choose Ragnir: black; the track fades in (if set); still 1 with its voice; each voiced still
   starts its voice; after the last still, black, the track fades out, the milestone's town track fades in.
3. New game with Zanyr: the placeholder intro is unchanged (its voice over the three stills).

- **Done, with deviations:** `ragnir_intro` wasn't in `content/cinematics.json` yet (CE6's hand check is the developer's), so this slice writes a placeholder: the six stills, 4 s each (the window's default), no subtitle or zoom, `Placeholder.wav` on still 1 (the voice today's intro plays); the developer times and words it in the window. The cut voice is remembered by its step instance and forgotten when the event ends, so an event run again plays its voice again. `ContentCatalogLoaderCinematicTests` moves to the test world (the repository's intro now names `ragnir_intro`, which its test file would leave out). The track tests are a sibling, `MusicSelectionCinematicTests`.
  CG3's review: `Update_EventRunAgain_PlaysItsVoiceUntilTheCut` tests that the cut is forgotten when the event ends; the five still-voice tests are in the sibling `EventPresenterCinematicVoiceTests` too, not CG2's file.
