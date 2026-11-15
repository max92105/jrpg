# CG5. The ring filling around the key's name

Game, about 150 lines raw (a new view type: about 240 at the 1.6x), about 40 of them tests. Needs CG4. Decision 626;
tunable "Ring"; `.claude/rules/game.md` "UI look" (shapes baked by `UiTextures` live in `Ui/`).

## Goal
While Confirm is held during a cinematic, a thin ring fills clockwise from the top around the key's name in the hint
("Hold [E] to skip", the key inside the ring), lit up to `SkipProgress`, the rest a dim track; released, it empties at
once. No ring while nothing is held (the dim track shows, so the player sees where it will fill).

## New types
- Game `Ui/RingSegmentShape` (internal sealed record): the ring's one segment as a small mask (a short, slightly curved
  bar the ring's thickness wide), baked once by `UiTextures` like `DiamondShape`.
- Game `Ui/ProgressRingView` (internal sealed): draws a ring of segments about a center, lit up to a share. 48 segments
  (decision 626), each the baked segment drawn with `SpriteBatch.Draw`'s rotation about the ring's center; the segment
  at the progress edge lit by its share of alpha; dim ones in `UiTheme`'s track color. No allocation per frame (the
  segment angles computed once). `Draw(SpriteBatch batch, UiTextures ui, TextureLibrary textures, Vector2 center, float
  share)`.

## Changes
- Game `Ui/UiTextures`: `RingSegment(TextureLibrary, RingSegmentShape)` baked once per shape.
- Game `Ui/UiTheme` (`.Dialogue` partial or the shared one): the ring's lit and track colors and its size, by role.
- Game `Exploration/StillView`: for a cinematic, the hint measured as its text around the key's name, the ring drawn
  centered on the key's name at `SkipProgress`.

## Tests
- `ProgressRingViewTests` (new, `tests/Jrpg.Game.Tests/Ui/`), through a small internal read seam (the lit count and the
  edge segment's alpha for a share), no drawing:
  - `LitSegments_ShareZero_IsNone`; `LitSegments_ShareOne_IsAll`; `LitSegments_ShareHalf_IsTwentyFour`;
  - `EdgeAlpha_ShareBetweenSegments_IsItsPart` (share 0.51: segment 24 at 0.48 of full).
- `RingSegmentShapeTests`: the mask's size and that its middle pixel is opaque and a corner transparent (as the other
  shapes' tests).

## Verify by hand
1. New game with Ragnir. With nothing held: a faint ring around E in the hint.
2. Hold E: the ring fills clockwise from the top over two seconds, smoothly (no visible jumps between segments), then
   the cinematic ends. Hold one second and let go: it empties at once.
3. F12 capture while half filled: the ring is round, even, and the key's name centered in it; compare with the hint's
   size at 1920 x 1080.

- **Note from CG4:** `CinematicRun.SkipProgress` (`_skip.Progress`) is this slice's to add, for the ring.

- **Done, with deviations:** the hint moves out of `StillView` into `Exploration/SkipHintView`, which lays it out in three parts (the words before the key, the key, the words after) so the ring has room around the key's name; `StillView` takes the `UiTextures` for it. `CinematicRun.Draw` passes its hold's progress to the ring, so no `SkipProgress` property. The ring shows only over a still: the black while the pictures load and before the first still draws no hint. The ring's segment is the ring's exact slice (smoothed at the inner and outer edges, cut hard at the sides). Test added: `LitSegments_ShareAboveOne_IsAll`, and the shape's center and inner-edge tests. Hand check (the look) is the developer's.
  CG5's review: `RingSegmentShape.Center` lies below the texture (its summary and test say so), `SkipRingLit`'s summary names the bright text color, and `Pixels_BesideTheSlice_IsTransparent` pins the slice's side cut. Left for the hand check: under point sampling the 2 px blocks overlap at the ring's inner edge, which may show a dotted brighter edge on the faint track (if it does, bake the track as one ring and keep segments for the lit part); and the key is found in the formatted hint by its label, which a sentence naming the same letter earlier would mislead (split the message on `{button}` if a text ever does).
