# EE2. One strip as typed: pace and hit frame boxes, the swing, its length, its problem

Editor, about 200 lines (about 100 of them tests, at about 10 lines each). Needs EE1. Decision 659.

## Goal
One effect strip as the window edits it: its file fixed (the image it describes), its frame time and hit frame kept as
typed, its swing chosen from the effects images or left empty for none. It shows its frame count (from the image's size,
or "image missing") and a length line ("12 frames, 0.20 s; hit at 0.16 s"). It says whether it differs from the unlisted
pace (80 ms, hit frame 0, no swing), which decides whether the window writes a record for it. Its problem names a box
that isn't a whole number first, then a hit frame past its frames, else nothing; Data's message for the whole list is
EE3's.

## New types
- `Editor/Battle/EffectStripViewModel` (internal sealed partial, `ObservableObject`): one strip's record as typed.
  - `static EffectStripViewModel From(EffectStripDefinition strip, Func<string, Point?> imageSizeOf)` and `static
    EffectStripViewModel Unlisted(string file, Func<string, Point?> imageSizeOf)` (the unlisted pace's boxes).
  - `string File`; `string FrameMillisecondsText`, `string HitFrameText` (through `Catalogs/TypedNumber`); `string
    SwingFile` (empty for none).
  - `int? FrameCount` (image width over height, null when the image is missing); `string LengthText`.
  - `bool IsListed` (its typed values differ from the unlisted pace); `string? Problem`; `EffectStripDefinition
    ToStrip()` (throws on a problem, as the other windows' `ToX`).
  - `PropertyChanged` raised for `LengthText`, `IsListed` and `Problem` when a box changes.

## Changes
- None outside the new type; `Catalogs/TypedNumber` is reused as is.

## Tests
- `EffectStripViewModelTests` (new, `tests/Jrpg.Editor.Tests/Battle/`), records from an `EffectStripFixtures` class
  (a 4-frame image of 8 x 8 frames, a lookup built in the test):
  - `From_Record_ShowsItsBoxes` (17 ms, hit frame 2, a swing: not the defaults).
  - `Unlisted_ShowsTheUnlistedPace`; `Unlisted_IsNotListed`.
  - `IsListed_FrameTimeChanged_IsTrue`; `IsListed_BackToTheUnlistedPace_IsFalse`.
  - `LengthText_FourFramesAt80_SaysTheLengthAndTheHit`; `FrameCount_ImageMissing_IsNone`.
  - `Problem_FrameTimeNotANumber_NamesTheBox`; `Problem_HitFramePastTheFrames_NamesIt`;
    `Problem_HitFrameOnTheLastFrame_IsNone` (the accepted bound).
  - `ToStrip_EmptySwing_IsNone`; `ToStrip_TypedSwing_IsTrimmed`.
  - `FrameMillisecondsText_Changed_RaisesProblemAndLength` (the relayed notices).

## Verify by hand
None: no window yet (EE4).

- **Done, with deviations:** the image size lookup is `Func<string, (int Width, int Height)?>`, as `PngSize` reads it,
  not a WPF `Point` of doubles; no `EffectStripFixtures` class, the test's own lookup and record being two lines. The
  length line reads "4 frames, 0.32 s; hit at 0.16 s" (two decimals).
- **Review:** the map row's entry moved past `EnemyViewModel`'s parenthetical; the frame time is compared parsed ("080"
  is the unlisted pace) and the length worked out in long milliseconds; tests added for `ToStrip` on a hit past the
  frames, each `IsListed` side and the hit frame's and swing's notices. 112 lines, tests 219.
