# MB8. A GIF imported as an attack effect

Core + Game + Editor, about 150 lines (about 60 of them tests). Needs MB6. Decision 639; tunable "Effect frame time", decision 57.

## Goal
In the Enemies window's Art tab, **Import...** beside **Attack effect** takes a GIF too, through the same options dialog
(height, background): its whole frames, scaled to the height, each padded to a square of its larger side (centered,
transparent around), side by side, become `<name>.png` in `content/art/effects/`, which Data's import accepts (whole
square frames, within 4096 px). The effect plays at the fixed 0.08 s a frame; the status line reports its total length
and the dropped GIF time: "FireExplosion.png: 12 frames, 0.96 s (effects play at 80 ms a frame; the GIF's 60 ms
dropped)."

## Changes
- `Editor/GifImport/FrameStrip`: `WriteSquarePng(...)` (each frame centered in a square of `max(width, height)`), beside
  `WritePng`: one method per behavior, no flag argument.
- `Editor/GifImport/GifConversion`: `ConvertToEffect(string gifPath, GifImportOptions options)` (the square strip and
  the frame count, no loop).
- `Editor/Catalogs/ArtImageImport`: a `.gif` for the effects folder takes `ConvertToEffect`; the result carries no
  animation, only the note with the length (frames x `EffectFrameTime`). The 0.08 s is the game's `EffectStrip` frame
  time, which the editor can't reference: Core gains `Battle/EffectTiming.FrameTime` (public, the one value), read by
  `Game/Battle/EffectStrip` and the import alike (decision 57's number in one place). It is the pace of a strip that
  `effect-strips.json` doesn't list: the battle turn presentation plan's EF1 (after this plan) folds it into
  `EffectStripDefinition.UnlistedFrameTime` and rewords the status line (its decision 666).
- `EnemyCatalogWindow.xaml.cs` `OnImportClicked`: the effects' dialog filter adds `*.gif`; the options dialog opens for
  it as for battlers.

## Tests
- `FrameStripTests`: `WriteSquarePng_WideFrames_PadsToSquares` (two 4 x 2 frames → 8 x 4, the padding transparent, the
  frame's pixels in rows 1 and 2); `WriteSquarePng_SquareFrames_LeavesThem`.
- `ArtImageImportTests`: `Import_GifIntoEffects_WritesWholeSquareFrames` (Data's check passes: the copy exists);
  `Import_GifIntoEffects_ReportsItsLength` (3 frames: "0.24 s").
- `EffectStripTests` (Game, existing): unchanged and passing (the frame time now read from Core).
- Core `EffectTimingTests`: none (a constant); the Game test above is its guard.

## Verify by hand
1. **Database > Enemies...**, choose the enemy whose attack should explode (decision 639, the shaman?), **Art** tab,
   **Import...** beside **Attack effect**, open `art-src\gifs\FireExplosion.gif`, keep or change the height, **OK**:
   the status line names the strip, its frames, its total length and the dropped time; the box shows
   `FireExplosion.png`. **OK**, **Ctrl+S**; add its `art-sources.json` line (`effects/FireExplosion.png`, `generated`).
2. In a battle with that enemy (MB7's local encounter), its attack plays the explosion over its target, about as long
   as the status line said.

- **Done, with deviations:** `ConvertedGif.Loop` is nullable (an effect has none) instead of a second result type;
  `GifConversion` shares its read, compose, clear and scale step between the two. The status note reads "Imported
  FireExplosion.png (8 frames, 0.64 s; effects play at 80 ms a frame, the GIF's 110 ms dropped).", the import's
  "name (note)" shape. The GIF tests went to `ArtImageImportGifTests`, MB5's sibling; a GIF for another folder is
  refused as "only a battle sprite or an attack effect".
  Review: no Critical; which folders take a GIF is `ArtImageImport.TakesGifs`, read by the window too; two summaries
  completed. The carried `GifFrame` equality note went to the backlog.
