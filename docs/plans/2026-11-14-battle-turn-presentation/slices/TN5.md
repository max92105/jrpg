# TN5. The plaque's look: a small dark plaque with a hairline, hugging the word

Game, about 150 lines (about 60 of them tests; +60 only if the rounded ends are needed). Needs TN3. Decision 646 (Q10);
tunable "Plaque look".

## Goal
The banner is drawn as the clip's plaque, starting plain: a near-black navy rectangle at 85 % opacity with a 1 px
hairline along its top and bottom, hugging its text with 14 px on each side and 6 px above and below, the text at 24 px
in the body font, centered across the screen with its top at y 190, under the turn order. "Attack" makes a plaque about
100 x 36 px. Every banner (skills, items, Defend, Move) takes the same look.

Then compare an F12 capture with the clip's plaque. Only if its rounded ends are missed, add them in this slice (below);
otherwise the slice stops at the plain plaque and says so in its Done note.

## New types
- `Game/Ui/PlaqueLayout` (internal readonly record struct): where a plaque sits around a measured text. `static
  PlaqueLayout Around(Vector2 textSize, int centerX, int top)`; `Rectangle Plaque`; `Vector2 TextPosition`; the padding
  constants are the tunables.
- Only if the rounded ends are needed: `Game/Ui/PlaqueShape` (internal static): a rounded plaque end's pixels. `static
  Color[] LeftEnd(int height, int radius, Color fill)`, the right end the same texture mirrored; `UiTextures.PlaqueEnd`
  baked once (UiTextures about 259 after the cinematics plan → 267).

## Changes
- `Game/Ui/UiTheme.Battle`: `BannerPlaque` (the fill) and the hairline's color (or `HighlightLine`'s gradient, as
  `BattleOutcomeScreens` draws the escaped band's lines).
- `Game/Battle/BattleBannerView` (61 → about 70): drops `FramedWindowView`; measures at 24 px; draws the plaque from the
  plain pixel in the fill, the two hairlines, then the text, all at the banner's opacity. Its summary describes the
  plaque; the old "300 px" and "28 px" constants go (grep "300" and "framed window" in `Game/Battle/` comments).

## Tests
- `PlaqueLayoutTests` (new): `Around_Text_PadsEachSide` (a 72 x 24 text: 100 x 36); `Around_Text_CentersOnTheGivenX`;
  `Around_Text_PlacesTheTextInsideThePadding`.
- Only with the rounded ends, `PlaqueShapeTests` (new): `LeftEnd_OuterCorner_IsClear`; `LeftEnd_InnerEdgeMiddle_IsSolid`;
  `LeftEnd_CornerArc_IsPartlyCovered`; `LeftEnd_TopAndBottom_AreMirrors`.

## Verify by hand
1. In battle, "Attack" sits in a small dark plaque just wide enough for the word, a fine light line along its top and
   bottom, under the turn order, centered; F12 a capture and compare it with the clip's plaque (the corners).
2. A long skill name widens the plaque around it; "Sarxian defends." fits on one line.
3. The plaque pops in and out with no frame around it.

