# NU1. The damage number at a member's feet or above an enemy's tag: digits stepping down, hopping in turn

Game, about 260 lines (about 150 of them tests; 3 old tests replaced). Needs TN2. Decisions 650, 651, 652 (the devil's
advocate's C3, C4; Q3, Q7); tunables "Number font size" to "Critical".

## Goal
Every floating number appears at full size on its moment, centered on its target: a party member's row with its bottom
10 px below the feet line (the clip), an enemy's with its bottom 4 px above the top of the enemy's name tag, so number
and tag never overlap. Its digits sit tight, each 6 px lower than the one before. Digit k rises 36 px from 0.067 + k x
0.033 s over 0.033 s, stays at the top and falls over 0.033 s to land at 0.2 s (a third or later digit keeps 0.033 s at
the top and lands after: the 2-digit clip can't say); 0.57 s after it appeared the number vanishes at once. Drawn digit
by digit with the 54 px body font (the strips come in NU3), in its tone's color; a critical 1.4x and red-gold; "WEAK"
or "RESISTED" beside the row's right end, still, gone with it; a multi-hit's numbers stack 48 px higher.

## New types
- `Game/Battle/DigitRow` (internal readonly record struct): where each digit of a number sits along its row. `static
  DigitRow Of(string digits, ReadOnlySpan<float> advances, float spacing)` (`advances` indexed by digit value, 0-9);
  `float Width`; `float LeftOf(int index)`. Built once per number when the playback starts.

## Changes
- `Game/Battle/FloatingNumber`: `At`, `RiseDistance`, the pop and fade constants go; new `static float
  DigitOffsetAt(int digit, TimeSpan sinceShown)` (pixels below the row's rest line: the stagger plus the hop, negative
  while up), `static bool IsShownAt(TimeSpan sinceShown)` (from 0, before 0.57 s) and `static float
  RowBottomOf(Rectangle spot, Rectangle? tag)` (a member's feet without a tag, above the tag with one); `StackSpacing` 36
  → 48; `DigitSpacing` -2. The summary describes the motion and pins it to the clip's frames.
- `Game/Battle/FloatingNumberLook` is deleted (no reader left).
- `Game/Ui/UiTheme`: `FloatingCritical` becomes a red-gold placeholder (Q3); the 1.4x stays.
- `Game/Battle/EnemyTagView`: `Rectangle? TagAreaOf(Combatant combatant)` (an enemy's tag area, worked out once in its
  constructor; null for a party member), so the number reads the tag's place instead of recomputing it.
- `Game/Battle/FloatingNumbersView` (84 → about 120): constructor `(BattleTurnSequence sequence, BattlersView battlers,
  EnemyTagView tags, ScreenText text)`; the 54 px font; the ten digits measured once; each playback's numbers turned into
  `DigitRow`s once; `Draw` places each digit at its row's left plus `DigitRow.LeftOf`, its bottom from `RowBottomOf`
  (the target's `SpotOf` and `TagAreaOf`) and `DigitOffsetAt`; the cue at the row's right end on the rest line. `TopOf`
  is no longer read here (`CommandPanelView` still reads it).
- `BattleScene`: passes `_enemyTags` (built before the numbers view).

## Tests
- `FloatingNumberTests` (65): `At_Start_IsPopScale`, `At_PastHalfLife_IsFading` and `At_EndOfLife_HasRisenFullyAndIsGone`
  test the motion this slice replaces and go with it (said in the Done note). New, pinned to the clip's frames #80-#84
  on a two-digit number (offsets digit 0, digit 1):
  - `DigitOffsetAt_DuringTheWait_IsTheStagger` (0.05 s: 0, 6).
  - `DigitOffsetAt_Frame80_FirstUpSecondAtRest` (0.100 s: -36, 6).
  - `DigitOffsetAt_Frame81_BothUp` (0.133 s: -36, -30).
  - `DigitOffsetAt_Frame82_BothStillUp` (0.167 s: -36, -30).
  - `DigitOffsetAt_Frame83_BothLanded` (0.200 s: 0, 6).
  - `DigitOffsetAt_Frame84_AtRest` (0.233 s: 0, 6).
  - `DigitOffsetAt_ThirdDigit_LandsAfterTheSecond` (digit 2 at 0.2 s: still up; at 0.267 s: 12).
  - `IsShownAt_AtTheImpact_IsTrue`; `IsShownAt_JustBeforeItsLifeEnds_IsTrue`; `IsShownAt_AtItsLifeEnd_IsFalse`;
    `IsShownAt_BeforeItsMoment_IsFalse` (a backfire's number before its beat).
  - `RowBottomOf_NoTag_IsBelowTheFeet`; `RowBottomOf_Tag_IsAboveTheTag`.
  - `ColorOf_Critical_IsTheCriticalTint` stays (it reads the theme).
- `DigitRowTests` (new): `Of_TwoDigits_PutsTheSecondAfterTheFirstsAdvance` (advances 20 and 30, spacing -2: lefts 0
  and 18); `Width_ThreeDigits_IsTheAdvancesLessTheSpacing`; `Of_RepeatedDigit_UsesItsAdvanceTwice`.
- `EnemyTagViewTests` (new, in the `RepositoryScreenText` collection for its fonts):
  `TagAreaOf_Enemy_EndsBelowItsSpot` (its bottom 38 px under the spot's bottom); `TagAreaOf_PartyMember_IsNone`.

## Verify by hand
1. A goblin hits a party member: the number appears at the member's feet the instant the HP changes, white, the digits
   stepping down to the right; the first digit hops, the second follows, both land together, and the number disappears
   at once about half a second after it appeared, without rising or fading.
2. Hit a goblin: its number sits just above its name tag, never over it, and hops the same way.
3. A critical (bigger, red-gold), a heal (green), a multi-hit (stacked, readable during the hops), a weak hit ("WEAK"
   beside, still).
4. F12 during the hop and compare with the reference clip's number, frame by frame.

- **Done, with deviations:** the three `FloatingNumber.At` tests went with the motion they tested, as planned. The
  clip's frame is 1/30 s exactly, so the frame tests round each offset to the pixel. `FloatingNumbersView.Draw` lost its
  unused `TextureLibrary` (it no longer reads `TopOf`). `TagAreaOf` finds the enemy in the view's list rather than
  keeping a second table. A third digit's landing follows from "a frame at the top", one rule for every digit.
  Review: above a tag the row rises 6 px per digit past the first (`RowBottomOf` takes the digit count, a test per
  side), so a three-digit hit's last digit keeps clear of it too; `DigitOffsetAt`'s and the cue's comments corrected;
  `DigitRow` a plain `readonly struct`; `ActionPlayback`'s summaries say the amounts show, no longer pop.
