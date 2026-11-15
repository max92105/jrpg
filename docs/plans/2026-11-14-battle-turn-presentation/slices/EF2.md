# EF2. The startup check: each listed strip and swing in `effects/`, its hit frame within its frames

Data, about 140 lines (about 80 of them tests). Needs EF1. Decision 654.

## Goal
At startup the art check refuses an `effect-strips.json` record whose strip or swing is missing from
`content/art/effects/` (by exact name, as attack effects are), a swing that isn't a row of square frames, and a hit frame
at or past its strip's frame count. The editor still saves such a record and marks it (decision 442): only the game's
start refuses.

## Changes
- `Data/Art/ArtReferenceCheck.Battle.cs` (the animated-monsters plan's MB0 partial; reread its size): every listed strip
  and every swing by exact name in `effects/` (the shared `AddMissingByExactName`, named by "effect-strips.json '<file>'"
  and "... swing"); the swings join the square-frame check (`AddPartialFrameStrips`); each listed strip's hit frame
  below its frame count (width over height), the message naming the file, the hit frame and the frame count. A listed
  strip that is missing is reported once (the exact-name line), not again for its frames.

## Tests
- `ArtReferenceCheckEffectTests` (new, `tests/Jrpg.Data.Tests/Art/`), art roots built as `ArtReferenceCheckTests` builds
  them (a strip of four 8 x 8 frames): `Check_ListedStripMissing_NamesIt`; `Check_ListedStripMissing_IsReportedOnce`;
  `Check_HitFrameOnTheLastFrame_Passes` (hit frame 3); `Check_HitFramePastTheLastFrame_NamesItsFrames` (hit frame 4: "4",
  "4 frames"); `Check_SwingMissing_NamesTheStripThatNamesIt`; `Check_SwingNotSquareFrames_NamesIt`.

## Verify by hand
1. Run the game: starts as before.
2. Put `[{ "file": "AttackEnemy.png", "frameMilliseconds": 80, "hitFrame": 99, "swingFile": "Nope.png" }]` in
   `content/effect-strips.json` and run: the start stops naming the hit frame past the frames and the missing swing.
   Revert the file.
