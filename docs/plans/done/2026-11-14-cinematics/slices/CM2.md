# CM2. The startup checks read the cinematics

Data, about 150 lines (about 100 of them tests). Needs CM1. Design: "What exists" (Data), key decision 2.

## Goal
The game refuses to start on a cinematic naming a picture, voice or track that isn't there by its exact name, naming
the cinematic and the still, as it does for a still step today. The editor still saves such a cinematic (decision 442).

## Changes
- `Data/Art/ArtReferenceCheck.FindProblems` (261 → about 267): after the still steps, every cinematic's stills, in id
  order then still order, through `AddMissingByExactName(problems, artRoot, ArtFolders.Cinematics, ...)` with `NamedBy`
  `cinematic 'ragnir_intro' still 3` (counted from 1, as the window shows them; CE3 numbers rows the same way). The
  summary's list names "every cinematic's still".
- `Data/Audio/AudioReferenceCheck.FindProblems`: `CinematicVoices(catalog)` (every still's `VoiceFile` that is set, in
  the voices folder, named as above) and `CinematicTracks(catalog)` (every cinematic's `MusicFile` that is set, in the
  music folder, `cinematic 'ragnir_intro'`). The summary names them.
- `docs/map.md`: the `Art` and `Audio` rows name the cinematics.

## Tests
- `ArtReferenceCheckCinematicTests` (new sibling; `ArtReferenceCheckTests` is at 239), on the test world's catalog
  `with { Cinematics = ... }` built in the test (`CinematicFixtures` in `tests/Jrpg.Data.Tests/Cinematics/`, shared
  with CM1's tests), art from a temp folder as the sibling files build it:
  - `FindProblems_CinematicStillMissing_NamesCinematicAndStill` (message pinned: `cinematics/test_cinematic/b.png: not
    found in ... Named by cinematic 'test_cinematic' still 2.`).
  - `FindProblems_CinematicStillInItsFolder_IsFine` (`test_cinematic/a.png` present).
  - `FindProblems_CinematicStillOtherCase_NamesTheRealName` (`A.png` on disk for `a.png`).
- `AudioReferenceCheckTests` (119 → about 160):
  - `FindProblems_CinematicVoiceMissing_NamesCinematicAndStill`.
  - `FindProblems_CinematicStillWithoutVoice_IsFine` (the filter's other side: a still with no voice next to one with a
    present voice).
  - `FindProblems_CinematicTrackMissing_NamesCinematic`.
  - `FindProblems_CinematicWithoutTrack_IsFine`.

## Verify by hand
None: `cinematics.json` is empty. A throwaway edit adding a cinematic naming `ragnir_intro/7_missing.png` makes the
game's start name it, then revert it (optional).

- **Done, with deviations:** both checks read the stills through a new Data `Cinematics/CinematicStills.Of` (every still with its `cinematic 'id' still n`), as they read steps through `MapEventSteps`. The audio tests are a sibling, `AudioReferenceCheckCinematicTests` (with a present-track test beside the missing one), and each test file builds its own cinematic: no shared `CinematicFixtures`.
