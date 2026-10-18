# 0009. Art location and asset loading

Status: accepted
Date: 2026-10-18

## Context
Slice 1c draws the first text and 3a the first map, so the Game needs a way to load art and fonts. Part 1 deferred the MonoGame content pipeline to here. What the choice has to fit:
- **Formats and use.** The art is 69 PNGs and 2 GIFs in `Assets/`: tilesheets, battlers, portraits, icons, backgrounds and the map sprite `player.png`. Content JSON names these files (`battleSpriteFile`, `portraitFile`, `iconFile`, `backgroundFile`, `attackEffectFile`).
- **The editor.** The WPF editor (11b) must show the same tiles and objects. It can read PNG but not `.xnb`.
- **Fonts.** None exist yet. Decision 46 approved `FontStashSharp.MonoGame` (MIT) and openly licensed fonts only.
- **Debug content.** In Debug both apps read the repository's `content/` directly (decision 77), so an art change shows on the next start without a rebuild.
- **Solo developer.** One build step less is one thing less to break.

## Options
### A. MonoGame content pipeline (`.mgcb`, compiled `.xnb`)
- Pros: the standard MonoGame route. Textures are processed at build time (premultiplied alpha, optional compression), and `SpriteFont` comes built in.
- Cons:
  - It needs two more packages (`MonoGame.Content.Builder.Task`, `dotnet-mgcb`) and a `.mgcb` file listing every asset by hand.
  - The editor can't read `.xnb`, so PNGs stay the source and every asset exists twice.
  - An art change needs a rebuild, which breaks "edit content, restart".
  - `SpriteFont` bakes one size per font, and text gets blurry when scaled.

### B. Runtime PNG through one `TextureLibrary`, TTF fonts through FontStashSharp
- Pros:
  - The game and the editor read the same files, with no build step and no duplicates.
  - Fonts render crisp at any size from one TTF.
  - One class loads every texture (`game.md`: one asset loader). It premultiplies alpha on load and caches by file.
- Cons:
  - The game loads images itself (`Texture2D.FromStream`), so a missing file is found at runtime unless Data checks references first (`ArtReferenceCheck`).
  - There is no build-time texture compression. That doesn't matter at this art size.
  - The project depends on FontStashSharp (approved, decision 46).

### C. Runtime PNG plus bitmap fonts with our own loader
- Pros: no font package.
- Cons: we write and maintain a glyph-atlas loader and a text layout. Each size is its own bitmap, so fonts need tooling we don't have. A worse version of B.

### D. Runtime PNG, content pipeline for `SpriteFont` only
- Pros: no third-party font package.
- Cons: it keeps all of A's tooling for one asset kind, and gets the fixed font sizes with it.

## Decision
**B** (developer, 2026-10-18; consistent with decision 46, which approved `FontStashSharp.MonoGame`).

The rules that go with it:
- **One art root:** `content/art/<kind>/`, with the kinds `tilesheets`, `field-sprites`, `battlers`, `portraits`, `icons`, `backgrounds`, `effects`, `fonts` and `ui`.
- **References by file name only:** content names a file within its kind (`"portraitFile": "Kamilia.png"`), never a path. Data's `Art/ArtFolders` maps each kind to its folder.
- **Until the cutover (slice 10):** art stays in `Assets/`.
  - In Debug, the art root is the repository's `Assets/` (build-time `ArtRoot` metadata), with the legacy folder names (`Tiles`, `player.png` at the root).
  - Slice 10 moves the files and switches the folder names; no other code changes.
  - Fonts are new files and live in `content/art/fonts/` from the start.
- **Fonts:** only under an open license (SIL OFL), with the license file next to the font.
- **Animated effects:** the GIF attack effects become PNG strips of square frames (part 3b, question 57). There is no GIF decoder.
- **Checking references:** Data's `ArtReferenceCheck` checks that every referenced art file exists. It runs at startup, outside `ContentCatalogLoader`, so content tests don't need art; only `Jrpg.Data.Tests` links art.

## Consequences
- Easier:
  - One copy of each image, read by both apps.
  - Changed art shows on the next start.
  - Text stays crisp at any size and any window scale.
- Harder:
  - Missing or misnamed art is caught at startup by `ArtReferenceCheck`, not at build time. The check must cover every `...File` field as new kinds appear.
  - Textures stay in memory once loaded. That's fine at 71 files; revisit if art grows into hundreds of megabytes.
- Watch:
  - `TextureLibrary` premultiplies alpha on load. Drawing with `BlendState.AlphaBlend` (the default) assumes it, and forgetting it gives dark fringes.
  - `TextureLibrary` must be the only place that opens image files (`game.md`).
