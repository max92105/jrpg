# EF7. The golden slash and impact in the repository

Content + hand import, about 60 lines (no new code; the tests re-pinned in EF3 and EF5 already read the records). Needs
EE4 and the animated-monsters plan's MB8. Decisions 654, 655, 657, 664; tunables "Golden slash pace", "Golden impact
pace", "Import heights".

## Goal
Every enemy and party member attacks with the developer's golden art: `GoldenSlashImpact.png` as its impact strip,
whose record names `GoldenSlashAttack.png` as its swing; both strips of square frames in `content/art/effects/`, listed
in `art-sources.json` and paced in `effect-strips.json` (the slash at 17 ms a frame, the impact at 80 ms with its burst,
frame 2, on the hit). The old `AttackEnemy.png` and `AttackPlayer.png` stay, unused (decision 664).

## Before the slice: the developer imports and paces the two strips
If `art-src/gifs/GoldenSlashAttack.gif` and `GoldenSlashImpact.gif` aren't committed yet, commit them first (they are
sources, never read by the game).
1. Run the editor (`dotnet run --project src/Jrpg.Editor`), **Database > Enemies...**, choose **goblin**, **Art** tab.
2. **Import...** beside **Attack effect**, open `art-src\gifs\GoldenSlashAttack.gif`. In the import dialog set the
   battle height to **340** (12 frames x 340 = 4080 px, under the 4096 px limit), leave the background box unchecked
   (check it only if the frames show a solid backdrop), **OK**: the status line names `GoldenSlashAttack.png` and its
   12 frames.
3. **Import...** beside **Attack effect** again, open `art-src\gifs\GoldenSlashImpact.gif`, height **480** (8 x 480 =
   3840 px), **OK**: the Attack effect box shows `GoldenSlashImpact.png`.
4. For **goblin_chief**, **stone_beetle** and **training_dummy**: pick `GoldenSlashImpact.png` in **Attack effect**.
   **OK**.
5. **Database > Effect strips...**: choose `GoldenSlashAttack.png`, **Frame time (ms)** **17**; choose
   `GoldenSlashImpact.png`, **Frame time (ms)** **80**, **Hit frame** **2**, **Swing** `GoldenSlashAttack.png`. **OK**.
6. **Ctrl+S** in the main window. Tell the implementer the slice can start.

## Changes (the implementer)
- `content/party-members.json`: every member's `attackEffectFile` becomes `GoldenSlashImpact.png` (the file isn't
  written by the editor; its layout kept).
- `content/art/art-sources.json`: `effects/GoldenSlashAttack.png` and `effects/GoldenSlashImpact.png` under the source
  the developer names (`generated` if made with the image generator), and `content/art/CREDITS.md` if the source is new.

## Tests
- No new test: `ContentCatalogLoaderTests` (the repository loads), the startup art check over the repository (EF2: both
  strips exist, square frames, hit frame 2 under 8 frames, the swing present), `ArtSourcesTests` (both listed) and
  `ContentWriterRepositoryTests` (the enemies and strips files as the editor wrote them) guard the content. Run the
  whole suite: the tests that pinned `AttackPlayer.png` were re-pinned in EF3 and EF5 to read the record.

## Verify by hand
1. A goblin's turn: "Attack", two blinks, the golden crescent sweeping in front of the goblin toward the party with its
   whoosh, a golden streak across the party member, then the burst exactly when the number and the HP change, the
   crescent glow and the sparks after. Compare side by side with the reference clip; tune the two paces in **Database >
   Effect strips...** by eye (Debug reads `content/` at each start).
2. Sarxian's attack: the same, mirrored, toward the goblins.
3. F12 captures at the slash and at the burst.

- **Done, with deviations:** the developer said the GIFs are AI-made, so both strips are listed under `generated`
  (already credited). The import ran headlessly through the editor's own `ArtImageImport` from a throwaway test,
  deleted after (heights 340 and 480, background kept: both GIFs are transparent, the strips' corners at alpha 0):
  `GoldenSlashAttack.png` 12 frames of 340 px, `GoldenSlashImpact.png` 8 of 480 px, its burst on frame 2. The records
  were written by hand in the writer's layout (`ContentWriterRepositoryTests` passes): every enemy's and party member's
  `attackEffectFile` is `GoldenSlashImpact.png`, paced 80 ms with hit frame 2 and the slash as its swing at 17 ms.
