# MB4. The frames scaled, the background removed on demand, written as a strip

Editor + Data, about 220 lines (about 120 of them tests). Needs MB3. Decisions 637, 638; tunables "Battle height",
"Widest image". Its caller is MB5's import.

## Goal
Headless transforms on whole frames: scale every frame to a height (aspect kept, resampled once), optionally make the
top-left pixel's color transparent, and write the frames side by side as one PNG. Data's art import refuses an image
wider than 4096 px for the battlers and effects folders, naming the width, for PNGs and converted GIFs alike.

## New types
- `Editor/GifImport/FrameScaler` (internal static): scales whole frames to a height, aspect kept (the width rounded to
  the nearest pixel, at least 1): nearest neighbor when the factor or its inverse is a whole number (pixel art stays
  crisp), an area average of the covered source pixels otherwise (alpha-weighted, so transparent edges don't darken).
  `IReadOnlyList<uint[]> Scale(IReadOnlyList<uint[]> frames, int width, int height, int targetHeight)` and `int
  WidthAt(int width, int height, int targetHeight)`; `bool IsWholeFactor(int height, int targetHeight)` for the dialog's
  note (MB6).
- `Editor/GifImport/BackgroundRemoval` (internal static): makes the top-left pixel's color transparent in every frame
  (the first frame's top-left color, exact match). `IReadOnlyList<uint[]> Apply(IReadOnlyList<uint[]> frames)`.
- `Editor/GifImport/FrameStrip` (internal static): whole frames side by side as one PNG strip. `void WritePng(
  IReadOnlyList<uint[]> frames, int frameWidth, int frameHeight, string path)` through `BitmapSource.Create` and
  `PngBitmapEncoder`.
- Data `Art/TextureLimit` (public static): the widest image side the game loads: `MaxSide = 4096` (HiDef's texture
  limit; the game sets no profile and MonoGame takes the highest supported).

## Changes
- Data `Content/ContentWriter.ArtImports.cs` (111 → about 121): `ImportArtImage` into battlers or effects refuses an
  image wider than `TextureLimit.MaxSide`: `GnollWarrior.png: a 4800 px wide image is wider than the game loads (4096
  px); import it at a smaller height.` The summary names the limit.

## Tests
- `FrameScalerTests` (new), frames built in the test:
  - `Scale_DoubleHeight_RepeatsEachPixelTwice` (nearest: a 2 x 2 checker becomes 4 x 4 blocks).
  - `Scale_HalfHeight_TakesEveryOtherPixel`.
  - `Scale_ThreeQuarters_AveragesCoveredPixels` (a 4 x 4 of black and white columns to 3 x 3: grey between).
  - `Scale_AverageNextToTransparent_KeepsTheColor` (an opaque red beside transparent averages to red at half alpha).
  - `WidthAt_KeepsTheAspect` (100 x 128 to 192: 150).
  - `Scale_SameHeight_IsUnchanged`.
  - `IsWholeFactor_TwoAndHalf_AreWhole`, `IsWholeFactor_ThreeQuarters_IsNot`.
- `BackgroundRemovalTests` (new): `Apply_TopLeftColor_BecomesTransparentInEveryFrame`; `Apply_OtherColors_AreKept`;
  `Apply_TopLeftAlreadyTransparent_ChangesNothing`.
- `FrameStripTests` (new), through `StaThread.Run`: `WritePng_ThreeFrames_IsThreeWideOneHigh` (read back through
  `PngSize`; the second frame's first pixel is the second frame's).
- Data `ContentWriterArtImportTests` (or the existing art import tests' file, Glob first): `ImportArtImage_BattlerWiderThan4096_IsRefusedNamingTheWidth`
  (a PNG header 4097 wide); `ImportArtImage_Battler4096Wide_IsCopied` (the accepted bound); `ImportArtImage_PortraitWiderThan4096_IsCopied`
  (other folders unchanged).

## Verify by hand
None: no button yet (MB5).

- **Done, with deviations:** `ImportArtImage_EffectWiderThan4096_IsRefused` added (the rule's other folder). The
  three-quarters tests assert a range around the average (126 to 129) rather than one value, since the covered shares
  are thirds in floating point.
  Review: a Critical fixed: an area's right or bottom edge, a step multiplied back, could land a hair past the source
  (53 x 100 to 192 threw), so the average read past the frame; `EdgeAt` divides the integer product last, and
  `Scale_UnevenStep_StaysInsideTheSource` pins it. `BackgroundRemoval` reads the first frame without a guard (the reader
  refuses a GIF of no frame); `TextureLimit` says why only the width is checked.
