# Animated monsters in battle: design

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Battle height | `Editor/GifImport/GifImportOptions.Default` | 128 px (decision 637) | A frame's height on screen after import; the dialog's starting value, editable per import. |
| Widest image | `Data/Art/TextureLimit.MaxSide` | 4096 px (decision 637) | The widest strip the import accepts: the HiDef profile's texture limit. A technical bound. |
| Short GIF delay | `Editor/GifImport/GifFrameTiming` | under 20 ms read as 100 ms (decision 632) | Browsers' rule for GIFs saved with 0 or 10 ms delays. |
| New idle boxes | `Editor/Battle/EnemyArtViewModel` | not animated; 2 frames, 100 ms when checked | What the boxes show for an enemy without a loop. Editor only. |
| Step toward the other side | `Game/Battle/BattlePacing.StepDistance`, `Step` (exist) | 50 px, 0.2 s (decision 636) | A party member's step; enemies act in place (the battle turn presentation plan's decision 643). Unchanged here. |
| Loop offset | `Game/Battle/BattlerFrames` | one frame per place (decision 635) | How far each enemy's loop starts from the first's. |
| Effect frame time | `Game/Battle/EffectStrip` (exists) | 0.08 s | An imported explosion plays at it (decisions 57, 639) until an `effect-strips.json` record paces it (the battle turn presentation plan's EF1). |

## Design overview

### What exists
- **Core** `Battle/EnemyDefinition` (`BattleSpriteFile`, `IsSpriteFlipped`, `AttackEffectFile`, no animation);
  `Maps/TileAnimation` (`FrameCount` from 2, `FrameMilliseconds` from 1, `FrameAt(elapsed)` in long ticks, throwing on a
  negative time), reused by `Sprites/FacingCycle` for the field sprites' walk (ADR 0043). `Battle` reads no `Maps` type
  today and `Maps` reads no `Battle` type: the new edge makes no cycle.
- **Data** `Battle/EnemyCatalogValidator` (59 lines; `InvalidEnemies` theory in its tests); the animation rule written in
  `Maps/TileCatalogValidator` and `Sprites/FieldSpriteCatalogValidator`; `Art/ArtReferenceCheck` (261 lines) checks
  enemies' battle sprites by exact name and effect strips as whole square frames (`IsRowOfSquareFrames`);
  `Content/ContentWriter.ImportArtImage` (battlers: any PNG; effects: whole square frames); `Content/AbsentWhenNullFilter`.
- **Game** `Battle/BattlersView` draws each combatant's whole image at its spot, sized by `BattlerScaling.Of(image
  size)` (over 512 px: shrunk once to the 96 px spot through `UiTextures.Battler`; pixel art: half size, point sampled),
  flipped by `BattlerArt.IsFlipped`, a party actor stepped out by `BattleTurnSequence.StepShare` (enemies act in place),
  whitened by `BattlerWhiteness` (the fallen as they fade, the actor's two blinks with their spill copies, through
  `UiTextures.WhiteSilhouette`; the battle turn presentation plan's phase 1). `BattlerArt` is the one place a combatant maps to its art. `EffectStrip`
  plays square-frame strips at 0.08 s a frame. The game sets no `GraphicsProfile`.
- **Editor** the Enemies window (`Battle/`): `EnemyArtViewModel` (the three pictures and the flip), the Art tab's
  Import... per folder (`Shell/MainViewModel.ImportArtImages` → `Catalogs/ArtImageImport` → Data), the battle sprite
  preview read whole by `Sprites/SpriteImageFile.LoadFrom`; `EnemyCatalogWindow`'s constructor is at four parameters.
  `Tiles/TileAnimationTime` is the editor's animation clock. `Tiles/SheetImportResult` (imported names, refusals) is what
  an import returns. No test project has an STA helper.
- **Art** the GIFs in `art-src/gifs/` after the cinematics plan's CA0, read by MB3's step 0 through `GifReader` (every
  frame the whole canvas at offset 0, a transparency flag on each, disposal 2, its delays all equal):
  - `FireExplosion.gif`: 443 x 443, 8 frames, 110 ms; transparent around the flame.
  - `GnollArcher.gif`: 545 x 724, 4 frames, 260 ms; **drawn on a solid near-white background** (`FEFEFE`, frame 0
    has no transparent pixel): its import needs MB6's background box.
  - `GnollAxeWarrior.gif`: 543 x 724, 4 frames, 240 ms; transparent around the figure.
  - `GnollShaman.gif`: 543 x 724, 4 frames, 280 ms; transparent around the figure.
  - `GnollWarrior.gif`: 724 x 724, 3 frames, 220 ms; transparent around the figure.
  - `HyenaWarlord.gif`: 556 x 724, 4 frames, 260 ms; transparent around the figure.
  - `GoldenSlashAttack.gif` (362 x 362, 12 frames, 80 ms) and `GoldenSlashImpact.gif` (313 x 627, 8 frames, 80 ms),
    added since for the battle turn presentation plan's EF7: transparent around the streaks.

### Core changes
MB1: `Battle/EnemyDefinition.IdleAnimation` (`TileAnimation?`, init, null by default: a still sprite), summary "its
battle sprite's loop, frames side by side in one row at their drawn size (ADR 0047); null for a still sprite". The
`TileAnimation` summary names the enemy's loop.

MB8: `Battle/EffectTiming.FrameTime` (public static, 0.08 s, decision 57): an effect strip's frame time, moved out of
`Game/Battle/EffectStrip` so the editor's effect import can report a length (the editor never references Game).

### Content format
```json
{
  "id": "gnoll_warrior",
  "battleSpriteFile": "GnollWarrior.png",
  "idleAnimation": {
    "frameCount": 8,
    "frameMilliseconds": 100
  }
}
```
Left out while null. A strip is the frames side by side in one row: frame width = image width / frame count, checked at
startup; its height is the drawn height.

### Structure and folder dependencies
Core: `Battle` → `Maps` (new edge, `TileAnimation`); no cycle.

Data: `Maps/AnimationProblems` (MB1): one animation's rule under a field name, shared by the tile, field sprite and enemy
validators (the third copy, extracted). `Art/TextureLimit` (MB4): the widest image side the game loads.
`Art/ArtReferenceCheck.Battle.cs` (MB0): the partial.

Editor, new folder `GifImport/` (namespace `Jrpg.Editor.GifImport`), no view model:

| Type | Responsibility |
|---|---|
| `GifFrame` (MB3) | One frame as the file stores it: its patch of pixels, where it lies, its delay and its disposal. |
| `GifDisposal` (MB3) | What a GIF does with its canvas after a frame: keep it, clear the frame's rectangle, restore the one before. |
| `GifImage` (MB3) | A GIF as read: its canvas size and its frames in order. |
| `GifReader` (MB3) | Reads a GIF's canvas size and frames through WPF's `GifBitmapDecoder` and its metadata. |
| `GifFrameComposer` (MB3) | Composes whole frames from the patches by the GIF disposal rules. |
| `GifFrameTiming` (MB3) | One frame time for a loop from the frames' delays. |
| `FrameScaler` (MB4) | Scales whole frames to a height, aspect kept: nearest neighbor for a whole factor, area average otherwise. |
| `BackgroundRemoval` (MB4) | Makes the top-left pixel's color transparent in every frame. |
| `FrameStrip` (MB4, MB8) | Whole frames side by side as one PNG strip, padded to squares for an effect. |
| `GifImportOptions` (MB5) | What one GIF import asks: the battle height and whether the top-left color is background. |
| `GifConversion` (MB5) | One GIF into a strip file and its loop: read, composed, scaled, cleared, written. |

| From | To | Why |
|---|---|---|
| `Catalogs` | `GifImport` | `ArtImageImport` converts a `.gif` before Data's import. |
| `GifImport` | (no editor folder) | Leaf: WPF imaging and Core's `TileAnimation` only. |
| `Battle` | `GifImport` | The options dialog builds `GifImportOptions`. |
| `Battle` | `Tiles` | `TileAnimationTime` for the preview (`Sprites` does the same). |

Editor, `Battle/`: `EnemyArtSources` (MB2): what the Art tab reads and copies pictures with (the import, a battle
sprite's image by name, the animation clock), keeping `EnemyCatalogWindow` at four constructor parameters.
`GifImportOptionsViewModel` and `GifImportWindow` (MB6): the import's dialog (the height as typed, the background box).

Game, `Battle/`: `BattlerFrames` (MB7): which frame each combatant's sprite shows: its loop's frame at the battle's
time, one frame later per place, held once fallen. `BattlerArt` gains `IdleAnimation`; `BattlerScaling` gains the
strip's own-size case. No new Game folder edge.

### Key decisions
Decisions 631-642 in the README; the ones that shape the code: 631 (convert at import, the field), 632 (timing), 634 and
635 (when and how the loop plays), 637 (scale at import, own size in battle, the width limit), 638 (background), 642
(the split).

### Extension points
- Another action's loop (a step, a hit) is another `TileAnimation?` field beside `IdleAnimation` and a stage check in
  `BattlerFrames`; the party's is the same field on `PartyMemberDefinition` (decision 641). Neither is built.

### Replacement mapping

| Existing | Replaced by | When |
|---|---|---|
| `BattlersView` drawing the whole image | the frame's source rectangle from `BattlerFrames` | MB7 |
| `BattlerScaling.Of(image size)` for every battler | the same for a still battler; the frame's own size for an animated one | MB7 |
| The animation rule in two validators | `Data/Maps/AnimationProblems` | MB1 |
| The Art tab's PNG-only import | PNG or GIF | MB5 (battlers), MB8 (effects) |

## Risks
- **GIF quirks:** partial frames, disposal modes (4-7 are reserved), frames without a timing block, patches past the
  canvas, transparency and zero delays; WPF exposes them only through metadata queries (`/imgdesc/Left`, `/imgdesc/Top`,
  `/grctlext/Delay`, `/grctlext/Disposal`, `/grctlext/TransparencyFlag`, `/logscrdesc/Width`). MB3 tests each on pixel
  arrays and on small fixture GIFs committed as bytes; a GIF it can't read is refused with its name, never half-imported.
- **WPF imaging in tests:** decoding and encoding run on an STA thread the test starts (`StaThread.Run`, MB3).
- **Unknown art:** the GIFs' frame sizes and transparency are unread at planning (no byte reader was available); MB3's
  step 0 records them in this plan, and if a GIF has no transparency, decision 638's option is what makes it usable.
- **Resampling pixel art:** a battle height that isn't a whole factor of the GIF's frame height averages pixels (soft
  edges); the dialog shows the factor so the developer can pick a whole one.
- **Fallen silhouette:** `UiTextures.WhiteSilhouette` makes a white copy of the whole texture; drawn with the frame's
  source rectangle it whitens only that frame. Checked at MB7's hand check.
- **Art sources:** an imported strip fails `ArtSourcesTests` until its `art-sources.json` line is added (`generated`,
  decision 611): the commit adding the strips adds the lines.
- **Content:** a gnoll battle needs an encounter by hand (decision 640); hand checks edit `encounters.json` locally and
  revert.
- **Determinism:** none at risk (no randomness; the loop's time is the battle's update deltas). **Saves:** unchanged.
