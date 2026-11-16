# AN1b. The shared preview control, first in the Effect strips page

Editor, about 140 lines (control about 70, page changes about 30 net, tests about 40). Decision 986. Design:
design.md, sections 1 ("Preview pixels"), 2.4, 2.6 ("The shared preview") and 2.9. Builds on AN1a and the mode bar's
WD6 (`Battle/EffectStripCatalogPage`; if WD6 hasn't landed, the same change goes into `EffectStripCatalogWindow` and
WD6 carries it over).

## Goal
One control previews any animation the editor writes: the frames, Play or Pause, Step back and Step forward, and the
frame line. It reads its pace and hit frame one-way: its host keeps its own typed box, so the preview and what is
written can't differ. The Effect strips page's preview becomes it: its Frame time box stays the page's, and the hit
frame is marked.

## New types
- `Editor/Animation/FramePreview` (`UserControl`, XAML + code-behind): the shared control.
  - Dependency properties, one-way: `Frames` (`IReadOnlyList<BitmapSource>?`; a change calls `FramePlayback.Show`),
    `FrameMilliseconds` (`int?`), `HitFrame` (`int?`), each handed to its `FramePlayback`.
  - The image on black, nearest neighbor (`RenderOptions.BitmapScalingMode`), `Stretch="Uniform"` in the area its host
    gives it; under it "|<", Play or Pause (`IsPlaying`), ">|" and the frame line.
  - Per-frame work through `Hosting/RenderTick`, started on `Loaded` and stopped on `Unloaded` (the page recipe),
    the delta from `RenderingEventArgs.RenderingTime`; the image's source changes only when `ShownFrame` changes.
- `Editor/Animation/PreviewFrames` (internal static): makes the images a preview plays. `OfStrip(BitmapSource strip,
  int frameCount)`: the strip's frames left to right, each its width over the count (`CroppedBitmap`), none when the
  width doesn't split. AN2 adds `OfPixels`, AN6 `OfCells`.

## Changes
- `Editor/Battle/EffectStripCatalogPage.xaml`: the 320 px preview `Border` and its frame line give way to one
  `FramePreview` (320 x 360) with `FrameMilliseconds` bound to the chosen strip's new `FrameMilliseconds` and
  `HitFrame` to its new `HitFrame`; the Frame time box stays where it is.
- `Editor/Battle/EffectStripCatalogPage.xaml.cs`: the page's own `RenderTick`, `ShowFrame` and `OnRendering` go; the
  chosen strip's image is read as before and handed as `PreviewFrames.OfStrip(image, strip.FrameCount)`; the page no
  longer takes the animation clock (its opener in `MainWindow.Database.cs` stops passing it).
- `Editor/Battle/EffectStripViewModel` (153): `PreviewLoop` and `FrameLineAt` go; `int? FrameMilliseconds` and
  `int? HitFrame` (the typed boxes as numbers, null while not whole numbers) are raised with their texts.
- `docs/map.md`: an Editor `Animation` row; the Battle row says the Effect strips page's preview is the shared control.

## Tests
- `Editor.Tests/Animation/PreviewFramesTests` (new; on `StaThread`, a strip built with `BitmapSource.Create`):
  `OfStrip_ThreeSquareFrames_CutsThree`, `OfStrip_WidthNotSplitting_IsEmpty`.
- `Editor.Tests/Battle/EffectStripPreviewTests` (103 lines): its `PreviewLoop` and `FrameLineAt` tests are deleted
  (their rules are `FramePlaybackTests`' now); `HitFrame_TypedNotANumber_IsNull`, `HitFrame_Typed_IsIt` and
  `FrameMilliseconds_Typed_IsIt` replace them.

## Verify by hand
Agents may run it (effects are generated art).
1. Database > Effect strips..., `FireExplosion.png`: it plays; Pause holds the frame; ">|" moves one frame and the line
   reads "frame 4 of 8"; on the hit frame it adds ", the hit".
2. Type 300 in the page's Frame time: it slows at once from the frame it showed; Play resumes. OK, Save all:
   `effect-strips.json` holds 300 for it. Ctrl+Z, Save all.
3. Type "x": the preview holds its frame and the problem line names the frame time.
4. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` or binding-error line, no `RenderTick` report on close.
- **Note from AN1a:** `docs/map.md` already has the Editor `Animation` row (`FramePlayback`): add `FramePreview` and
  `PreviewFrames` to it. Null `Frames` is `Show(0)`; the three commands grey with no frames and the Steps' `Execute`
  divides by the count, so a key handler on the control checks `CanExecute` first.
- **Done, with deviations:** the control is 134 lines (plan about 70: three dependency properties and the render
  clock), `PreviewFramesTests` 48, `EffectStripPreviewTests` 77 with three more tests (`FrameMilliseconds` not a
  number, both numbers raised with their texts); the control forgets its last render time on Unloaded; no key handler.
