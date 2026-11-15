# CE3. One still as typed

Editor, about 190 lines (about 100 of them tests). Needs CM1. Design: "Structure" (Editor), tunables "New still's
seconds", "New still's zoom".

## Goal
A still's boxes keep what was typed; its problem says which box can't be read, numbered as the window shows it; `ToStill`
builds the record once there is none.

## New types
- `Editor/Cinematics/CinematicStillRow` (internal sealed partial, `ObservableObject`): one still as typed: picture,
  seconds, subtitle, voice and zoom boxes, and why a box can't be read.
  - `[ObservableProperty]` `ImageFile`, `SecondsText`, `Caption`, `VoiceFile`, `ZoomStartText`, `ZoomEndText` (each
    raising `Changed` for the list's problem relay, never `Label`/`Problem` notices, `.claude/rules/editor.md`).
  - `int Number` (from 1, set by the list; the row's label "3. ragnir_intro/3_son_becoming_soldier.png, 6 s").
  - `static CinematicStillRow From(CinematicStill still)`; `static CinematicStillRow New(string imageFile)` (4 s, zoom
    100 to 100, decision 616).
  - `string? Problem`: the picture empty ("Still 3: choose its picture."); seconds not a number above 0 (the
    `EventStepViewModel` parse: dot decimal, finite, `.claude/rules/editor.md`); a zoom box not a whole number
    (`Catalogs/TypedNumber`); range rules are Data's (`CatalogProblems.OfCinematics` in CE4).
  - `CinematicStill ToStill()`: trimmed names; empty caption and voice are null; zoom null when both boxes are 100
    (written as no zoom, decision 617), else a `StillZoom`; throws `InvalidOperationException(Problem)` while a problem shows.
  - `event EventHandler? Changed`.

## Changes
- `docs/map.md`: an Editor row for `Cinematics` (the window's rules; its types as they land).

## Tests
`CinematicStillRowTests` (new, `tests/Jrpg.Editor.Tests/Cinematics/`), rows built from records in the test:
- `From_StillWithEveryField_FillsTheBoxes` (zoom 100 to 108 → "100", "108"; voice, caption).
- `From_StillWithoutZoom_ShowsHundredTwice`.
- `New_Picture_HasFourSecondsAndNoZoom`.
- `ToStill_BoxesAsTyped_MakesTheStill` (a fixture different from `New`'s defaults: 5.5 s, zoom 110 to 100).
- `ToStill_ZoomHundredToHundred_HasNoZoom`.
- `ToStill_EmptyCaptionAndVoice_AreNull`.
- `Problem_NoPicture_NamesTheStillByNumber`.
- `Problem_SecondsInfinity_AsksForANumber` and `Problem_SecondsZero_AsksForANumber`; `Problem_SecondsPointFive_IsNull`.
- `Problem_ZoomNotANumber_NamesTheBox`.
- `Changed_TypingSeconds_IsRaised`; `Changed_NumberSet_IsNotRaised` (the relay listens to typed fields only).

## Verify by hand
None: no window yet.

- **Done, with deviations:** the seconds parse copies `EventStepViewModel`'s (its second copy; CR3 deletes the first with the still step). The seconds problem shows what was typed. Two tests more: `ToStill_WithProblem_Throws` and `Problem_NumberSet_IsRaised` (the number is in the problem's text).
