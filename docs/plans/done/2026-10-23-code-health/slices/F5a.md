# F5a. Field sprites must hold the walk frames; the Data tests share one PNG writer

Part of [the code health plan](../README.md). Design: [design.md](../design.md), tunables "Field sprite ...", key decision 6, risk "F5a may refuse today's content"; decisions 128, 130, ADR 0015.

- **F5a. Field sprites must hold the walk frames** (Data + Game + Editor, about 190 lines)
  - **First:** read the sizes of `content/art/field-sprites/player.png` and `npc_placeholder.png` (`PngSize.Read`). If either is smaller than 384 × 192, stop and ask the developer.
  - **Write first, and see it fail:** `ArtReferenceCheckTests: FindProblems_FieldSpriteSmallerThanTheWalkFrames_NamesItAndTheNeededSize`.
  - **Goal:**
    - Every field sprite the content names (the player's and each NPC object's) must hold 8 walk frames by 4 facings of 48 px, as the walk animation reads them. A smaller one stops the game at startup, naming the file, its size and the size needed. The editor's save refuses it the same way, since it runs the same check.
    - The frame size is written once, so the game, the editor and the check can't drift apart.
  - **New types:**
    - Data `Art/FieldSpriteLayout` (public static): the frame grid every field sprite sheet follows, and whether an image holds it.
      - `const int FramePixels = 48`, `const int WalkFrames = 8`, `const int Rows = 4`.
      - `static bool Holds((int Width, int Height) size)`: at least `WalkFrames × FramePixels` by `Rows × FramePixels`; larger is allowed.
  - **Changes:**
    - Data `Art/ArtReferenceCheck.FindProblems`: for the new game's field sprite and every NPC object's, a readable image that doesn't `Hold` the layout adds "field-sprites/<file>: a W×H sprite doesn't hold 8 × 4 frames of 48 px (384×192). Named by …". The summary lists it.
    - Data `Content/ContentWriter.WriteAll`: a comment on the move loop. The moves aren't atomic across files, so a crash between two moves leaves some files new and some old, and the next save rewrites them all (README, "Proposed by the architect").
    - Game `Exploration/FieldSpriteFrame`:
      - `Pixels` goes, and `SourceOf` uses `FieldSpriteLayout.FramePixels`;
      - the sentence saying the editor mirrors the frame size goes from the summary.

      Grep the other `FieldSpriteFrame.Pixels` callers and point them at the layout too.
    - Game `Exploration/WalkAnimation`: `FrameCount` becomes `FieldSpriteLayout.WalkFrames`.
    - Editor `Tiles/TileImages`:
      - `FieldSpriteFramePixels` and its comment go, and `FieldSpriteFrameOf` uses `FieldSpriteLayout.FramePixels`;
      - it still shows a smaller sprite whole, because the New NPC dialog can still pick one (backlog, "When next touched").
    - `tests/Jrpg.Data.Tests/Art/PngHeaderFile.cs` (new test helper, like the editor tests' one): writes a header-only PNG of a given size.
      - `ArtReferenceCheckTests.WritePngHeader` and `ContentWriterTests.WritePngHeader` go.
      - Their calls take the helper. Where a test passed only a file name, it now passes the full path, the only change in those bodies.
    - `docs/map.md`: the Data `Art` row lists `FieldSpriteLayout`.
    - `docs/plans/backlog.md`:
      - remove the `ContentWriter.WriteAll` line (the error-hiding half dropped by the review, the atomic half commented here);
      - remove the "header-only PNG writer is copied" line;
      - the `TileImages.FieldSpriteFrameOf` line becomes: "`TileImages.Forget` keeps cached field-sprite frames; drop them too once field sprites can be imported (no import yet)."
  - **Tests:**
    - `FieldSpriteLayoutTests`:
      - `Holds_ExactlyTheWalkFrames_IsTrue`;
      - `Holds_OneFrameShortInWidth_IsFalse`;
      - `Holds_OneRowShort_IsFalse`;
      - `Holds_Larger_IsTrue`.
    - `ArtReferenceCheckTests`:
      - `FindProblems_FieldSpriteSmallerThanTheWalkFrames_NamesItAndTheNeededSize`;
      - `FindProblems_NpcSpriteSmallerThanTheWalkFrames_NamesTheObject`;
      - `FindProblems_RepositoryArt_HasNoFieldSpriteProblem`.
    - The existing `FieldSpriteFrameTests`, `WalkAnimationTests` and `ContentWriterTests` pass.
  - **Verify by hand:**
    - [ ] Copy `content/` to a temp folder and replace `npc_placeholder.png` there with a 48 × 48 PNG. `dotnet run --project src/Jrpg.Game -- --content <temp>` stops at startup, naming the file and 384×192.
    - [ ] With the real content, the player and the villager walk and face as before.
- **Done, with deviations:** the two sprites are 384×288 (more than 4 rows), so they pass. The new game's sprite is "Named by the new game" in the problem line. The writer tests take a private `ImportSource(fileName)` path helper beside `TilesheetImage`, so each call keeps one line. Hand checks: a 48×48 `npc_placeholder.png` in a content copy stops the game with "field-sprites/npc_placeholder.png: a 48x48 sprite doesn't hold 8 x 4 frames of 48 px (384x192). Named by object 'villager'."; with the real content the player walks and the villager is drawn as before (screenshot).
