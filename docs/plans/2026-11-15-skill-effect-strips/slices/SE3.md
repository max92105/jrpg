# SE3. Fireball's and the bomb's explosion

Content, about 25 lines, plus a throwaway import test that is deleted before the commit (as the battle turn
presentation plan's EF7). Needs SE2. Decisions 711, 712; design.md "Tunables and placeholders".

## Goal
`content/art/effects/FireExplosion.png` exists, imported from `art-src/gifs/FireExplosion.gif` through the editor's own
GIF import; `effect-strips.json` paces it; `art-sources.json` credits it; Fireball and the bomb name it. Casting
Fireball or throwing a bomb plays the explosion on every enemy hit, its hit frame on the impact.

## What the GIF holds (already measured)
From the animated monsters plan (its design.md, and MB8's Done note): `FireExplosion.gif` is 443 x 443, 8 frames,
110 ms a frame, transparent around the flame. So (decision 712): import height **443** (8 x 443 = 3544 px, under
`TextureLimit.MaxSide`'s 4096), background box **off**, `frameMilliseconds` **110**. Any hit frame up to 6 keeps the
lead (hit frame x 110 ms) within the 0.7 s wind-up (decision 656).

## Step 1: the import (headless, through the editor's own code)
In a throwaway test in `tests/Jrpg.Editor.Tests` (run through `StaThread`, as the GIF import's tests run WPF imaging),
against the repository's `content/` (`ContentRootLocator`, as the editor finds it), build the import as
`MainViewModel.ImportArtImages` does: `new ArtImageImport(writer, catalog)` with a `ContentWriter` on the content root
and a `CatalogDraft` over the loaded catalog, then
`Import(ArtFolders.Effects, [gifPath], new GifImportOptions(443, false), _ => false)`. Assert the result lists
`FireExplosion.png` with no refusal, and that its note reads "8 frames". Delete the test file afterwards: it is never
committed, and `git status` shows only the files below.

The developer can do the same by hand instead:
1. Run the editor, **Database > Enemies...**, choose any enemy, **Art** tab.
2. **Import...** beside **Attack effect**, open `art-src\gifs\FireExplosion.gif`; in the import dialog set the battle
   height to **443**, leave the background box unchecked, **OK**. The status line names `FireExplosion.png` and its 8
   frames; the file is written into `effects/` at once.
3. **Cancel** the Enemies window: the enemy keeps its attack strip, the image stays.

Then look at `content/art/effects/FireExplosion.png` (the implementer reads the PNG) and pick the hit frame: the frame,
from 0, where the burst is widest (0 to 6).

## Changes
- `content/art/effects/FireExplosion.png`: written by the import.
- `content/effect-strips.json`: a record first, before `GoldenSlashAttack.png`'s (file-name order), in the writer's
  layout (one field per line, CRLF, as the file is): `"file": "FireExplosion.png"`, `"frameMilliseconds": 110`,
  `"hitFrame"` as chosen, no `swingFile`.
- `content/art/art-sources.json`: a two-line object in the file's layout, between `effects/AttackPlayer.png`'s and
  `effects/GoldenSlashAttack.png`'s:
  ```
      {
        "path": "effects/FireExplosion.png",
        "sourceId": "generated"
      },
  ```
  (decision 712; `generated` is already credited in `CREDITS.md`).
- `content/skills.json`: `"effectFile": "FireExplosion.png",` on its own line after `fireball`'s `soundFile` line
  (decisions 710, 711), the file's hand-written layout kept.
- `content/items.json`: `"effectFile": "FireExplosion.png",` on its own line after `bomb`'s `price` line (decision 711:
  the item path's user), the layout kept.

## Tests
- No new test. The repository guards hold the content: `ContentCatalogLoaderTests` (the repository loads; Fireball's
  and the bomb's strips pass SE1's validators), the startup art check over the repository (`FireExplosion.png` exists
  by exact name, square frames, the record's hit frame within its frames, SE1 and ADR 0048), `ArtSourcesTests` (the
  new file is listed) and `ContentWriterRepositoryTests` (`effect-strips.json` reads as the editor writes it). Run the
  whole suite.

## Verify by hand
1. New game as **Omer**, a random battle (`milestone_outside` or `primeval_growth_crossroad`). Once his Mana gauge
   holds 5 pips (his second turn), **Skills > Fireball**: the explosion plays on every goblin, starting during the
   wind-up, its hit frame exactly when the numbers land, the impact's light and fringe on that beat. No swing.
2. **Items > Bomb** on a goblin (the new game starts with bombs): the explosion on that goblin.
3. **Fire Blast** and **Attack** play as before (the golden impact; the slash before an Attack).
4. Tune by eye in **Database > Effect strips...**: choose `FireExplosion.png`, change **Frame time (ms)** or **Hit
   frame**, **OK**, **Ctrl+S**, restart the game (Debug reads `content/` at each start). Note any change in the Done
   note.
5. Watch the first explosion of the session for a stall while the strip loads (design.md, Risks); note it if seen.
6. F12 captures at the wind-up and at the burst.
