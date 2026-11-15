# MB3. A GIF read into whole frames and one frame time

Editor, about 240 lines (about 140 of them tests). Needs MB1 (`TileAnimation`). Decisions 631, 632. Design:
"Structure" (`GifImport/`), risks "GIF quirks", "WPF imaging in tests", "Unknown art". Its caller is MB5's import.

## Step 0: the transparency finding
Not checked at planning (no byte reader was available to the planner). Before writing code, read each of the six GIFs
in `art-src/gifs/` with the reader this slice writes (a throwaway test, deleted after): its canvas size, frame count,
delays, and whether its frames carry a transparent color (`/grctlext/TransparencyFlag`) or are drawn on a solid
background. Write the six lines into the design's "What exists" (Art) and, if any GIF has a solid background, say so
under decision 638 in the README: MB6's "top-left color is background" box is then needed for it.

## Goal
Headless: a GIF file becomes its whole frames (each the full canvas, BGRA pixels) and one frame time, whatever its
frames' patches, offsets, transparency, disposal and missing timing; a GIF it can't read is refused with its name.

## New types
- `Editor/GifImport/GifFrame` (internal sealed record): one frame as the file stores it: its patch of pixels (BGRA,
  row-major), where it lies (`Left`, `Top`, `Width`, `Height`), its delay in hundredths of a second (0 without a timing
  block) and its `Disposal`.
- `Editor/GifImport/GifDisposal` (internal enum): `Keep`, `ClearToBackground`, `RestorePrevious`. GIF's 0 and 1 read as
  Keep, 2 and 3 as named, the reserved 4 to 7 as Keep (as browsers do), a frame without a timing block as Keep.
- `Editor/GifImport/GifImage` (internal sealed record `(int Width, int Height, IReadOnlyList<GifFrame> Frames)`).
- `Editor/GifImport/GifReader` (internal static): reads a GIF's canvas size and frames through WPF's
  `GifBitmapDecoder` and its metadata (`/logscrdesc/Width`, `/logscrdesc/Height`, `/imgdesc/Left`, `/imgdesc/Top`,
  `/grctlext/Delay`, `/grctlext/Disposal`; a missing query reads as absent, never a throw), each frame converted to
  `Bgra32` (`FormatConvertedBitmap`). `GifImage Read(string path)`; throws `InvalidDataException` naming the file on
  anything WPF can't decode.
- `Editor/GifImport/GifFrameComposer` (internal static): composes whole frames from the patches by the disposal rules:
  each patch drawn over the canvas where it lies, clipped to the canvas (a patch past the edge loses what lies outside),
  transparent pixels leaving what's under them; after a frame, Keep leaves the canvas, ClearToBackground clears that
  frame's rectangle (clipped) to transparent, RestorePrevious puts the canvas back as it was before the frame.
  `IReadOnlyList<uint[]> Compose(GifImage image)`.
- `Editor/GifImport/GifFrameTiming` (internal static): one frame time for a loop from the frames' delays (decision
  632). `int FrameMilliseconds(IReadOnlyList<GifFrame> frames)` and `bool AreDelaysEqual(...)` for the import's note.
- `tests/Jrpg.Editor.Tests/StaThread.cs` (test helper, internal static): `Run(Action body)`: runs the body on a new
  thread set to `ApartmentState.STA`, joins it, and rethrows its exception with its stack (`ExceptionDispatchInfo`), so a
  failed assert inside fails the test. Every test touching WPF imaging (this slice's reader, MB4's strip writer, MB5's
  import) runs through it.

## Changes
- `docs/map.md`: an Editor row for `GifImport`.

## Tests
- `GifFrameComposerTests` (new, `tests/Jrpg.Editor.Tests/GifImport/`), on 4 x 4 canvases built in the test:
  - `Compose_FullFrames_KeepsEachAsIs`.
  - `Compose_PatchAtOffset_DrawsOverThePreviousFrame` (frame 2 a 2 x 2 patch at (1, 1)).
  - `Compose_TransparentPatchPixel_LeavesWhatIsUnder`.
  - `Compose_ClearToBackground_ClearsOnlyThatFramesRectangle`.
  - `Compose_RestorePrevious_PutsTheCanvasBack` (frame 3 sees frame 1's canvas, not frame 2's).
  - `Compose_PatchPastTheEdge_IsClipped` (a 3 x 3 patch at (2, 2): only its top-left 2 x 2 lands, no throw).
  - `Compose_ClearPastTheEdge_ClearsTheClippedRectangle`.
- `GifFrameTimingTests` (new): `FrameMilliseconds_AllEqual_IsThatDelay` (8 cs → 80); `FrameMilliseconds_Mixed_IsTheMostCommon`;
  `FrameMilliseconds_TieInCount_IsTheFirstInFrameOrder`; `FrameMilliseconds_NoTimingBlock_IsHundred` (delay 0);
  `FrameMilliseconds_TenMilliseconds_IsHundred` and `FrameMilliseconds_TwentyMilliseconds_IsTwenty` (both sides of the
  bound); `AreDelaysEqual_Mixed_IsFalse`.
- `GifReaderTests` (new), each through `StaThread.Run`, on GIFs committed as bytes in `GifFixtures` (each written once
  with a hex dump, its frames, offsets, delays and disposals listed in a comment):
  - `Read_ThreeFrameFixture_ReadsSizeFramesOffsetsAndDelays`.
  - `Read_FrameWithoutTimingBlock_HasNoDelayAndKeeps`.
  - `Read_ReservedDisposalFive_ReadsAsKeep` (and a row for 4 and 7 if the fixture holds them).
  - `Read_PatchPastTheCanvas_ReadsItsOffsetAsStored` (the composer clips it).
  - `Read_NotAGif_ThrowsNamingTheFile`.
- `StaThreadTests` (new): `Run_BodyThrows_RethrowsIt`; `Run_Body_RunsOnAnStaThread`.

## Verify by hand
None: no button yet (MB5). Step 0's findings are written in the plan.

- **Done, with deviations:** step 0 read eight GIFs (two golden effects were added since) through `GifReader` and the
  composer in a throwaway test, deleted: `GnollArcher.gif` is drawn on a solid near-white background, so MB6's box is
  needed for it (design "What exists", decision 638). The fixture GIFs are written by a small encoder in the test,
  `GifFixtures` (a clear code before every pixel, so every code is 3 bits), rather than hex dumps: each test states its
  frames in code. `Read_TransparentColor_HasNoAlpha` and `AreDelaysEqual_ShortDelaysPlayingAlike_IsTrue` added (the
  equality compares delays as played).
  Review: a Critical fixed: a patch wholly right of or below the canvas cleared with a negative length (WPF reads such
  an offset as stored); `ClippedArea` now gives an empty area, `Compose_ClearWhollyOffTheCanvas_LeavesTheCanvas` added.
  `FixtureFrame` in its own file; `GifFrame.Pixels` says it compares by reference.
