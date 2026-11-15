# Animated monsters in battle
Status: done

Drafted on 2026-11-14 from the developer's request (cleaned up by the caller):

> Animate monsters in battle. I added animated GIFs for monsters (a gnoll archer, a hyena warlord boss with a flail, a
> gnoll axe warrior, a gnoll warrior, a gnoll shaman) and a fire explosion, probably a skill effect. For now a monster's
> battle animation is in place only: an idle loop, plus "move up" and "move back" (a short step toward the party when it
> acts, then back). Other animations come from the skill and attack animations (the battle effects). The player side
> will get more steps (move right or left) later: out of scope here.

Approved on 2026-11-14 with the developer's answers: the editor converts GIFs on import (ADR 0047, option D weighed);
each frame scaled at import to a battle height in screen pixels, editable per import, and an animated strip drawn at
its own size (decision 637); an optional background removal (decision 638); MB3's edge cases and STA runner; the
effect import reports its length; `ArtReferenceCheck` split first (MB0).

Runs after [`2026-11-14-cinematics`](../2026-11-14-cinematics/README.md) and **depends on its CA0**, which moves the
GIFs to `art-src/gifs/` under their new names (decisions 612, 633) and lists them as `generated` art (decision 611).

Also runs after phase 1 (TN0-NU1) of [`2026-11-14-battle-turn-presentation`](../../2026-11-14-battle-turn-presentation/README.md)
(its decision 662): by MB7 enemies act in place and blink in their wind-up; that plan's phases 2-4 run after this one,
its EF1 folding MB8's `EffectTiming` and its EF7 importing the golden GIFs through MB8 (amended at its approval, its
decision 666).

Builds on ADR 0009 (art as PNGs loaded at runtime, "the GIF attack effects become PNG strips of square frames", no GIF
decoder), decision 57 (effect strips), decision 100 (battler scaling), ADR 0013 (battle playback), ADR 0039 (an
animation's shape: frame count and one frame time), ADR 0043 (field sprites' walk reuses it), ADR 0045 (the Enemies
window), ADR 0038 (art sources).

- Numbers: decisions 631-642, ADR 0047.
- What exists already when this plan runs: the battle turn presentation plan's phase 1 has made enemies act in place
  (its decision 643) and brighten twice in their wind-up instead of moving (its decision 648, `BattlerWhiteness`); only a
  party member steps (`Game/Battle/BattlePacing.StepDistance` 50 px over `Step` 0.2 s, `BattleTurnSequence.StepShare`,
  drawn by `BattlersView.SteppedSpot`). The developer's "move up" and "move back" became that flash; this plan adds the
  idle loop and the way GIFs become game art (decision 636).
- Core: `EnemyDefinition.IdleAnimation` (ADR 0039's shape). Data: its check, the strip's startup check, an image width
  limit at import. No save change.

## Goal
1. **A GIF becomes game art in the editor:** in the Enemies window's Art tab, **Import...** beside the battle sprite
   takes a GIF: a small dialog asks the battle height (default 128 px) and whether the top-left color is background;
   the frames, composed whole and scaled to that height, become one PNG strip in `content/art/battlers/`, and the
   enemy's idle animation boxes fill from the GIF. No GIF reaches `content/`, no package is added.
2. **An enemy declares its idle loop** (`idleAnimation`: frame count and frame time), set and previewed, animated, in
   the Enemies window.
3. **The battle plays it:** each animated enemy loops its frames in place at the strip's own size, all the time, one
   frame apart from its kin; the loop plays in place through the enemy's own turn (enemies don't step, the battle turn
   presentation plan's decision 643); a fallen enemy holds its frame while it fades. Battlers imported as PNGs draw as
   today.
4. **The explosion** becomes an attack effect strip of square frames through the same import, its total length shown.

## Order, and why
1. **MB0, a pure move:** `ArtReferenceCheck.cs` (261, about 267 after the cinematics plan's CM2) would pass 300 with
   MB1's strip check; its battle-art part moves to a partial first (decision 642).
2. **MB1, the content field** (Core + Data): what everything else reads.
3. **MB2, the Enemies window's boxes and animated preview**, so the field is set and seen before any GIF exists.
4. **MB3-MB6, the GIF import**: reading and composing (headless, the riskiest part, with the transparency finding
   recorded first), then the transforms (scale, background, strip, width limit), then the import itself with default
   options, then the options dialog. After MB6 the developer imports the five gnolls and makes their enemies (content
   work, decision 640).
5. **MB7, the battle draws the loops**: the payoff, with real gnolls to look at.
6. **MB8, the explosion** as an effect strip.

## Guiding rules
- **No runtime GIF:** the game reads PNG strips only (ADR 0009); the editor converts through WPF's own
  `GifBitmapDecoder` and `PngBitmapEncoder` (no package, no `System.Drawing`); composing, scaling and background removal
  are pure code on pixel arrays, tested headless.
- **One time rule:** a loop's frame at a time is Core's `TileAnimation.FrameAt` (ADR 0039), read by the game and the
  editor's preview; Core never reads a clock.
- **The editor never locks out on missing art** (decision 442): an enemy naming a strip the folder lacks is marked and
  saved; the game's startup refuses it, and refuses a strip whose width isn't whole frames.
- **Today's art keeps working:** a battler without a loop draws by decision 100's path, unchanged.
- **No allocation per frame:** frame source rectangles from numbers; nothing built in `Draw`.
- **Sizes:** Editor slices about 200 lines with tests at about 10 lines each; MB7 adds no view type.
- **Files near 300** (2026-11-14): `Data/Art/ArtReferenceCheck.cs` (MB0 splits it); `Editor/Battle/EnemyCatalogWindow.xaml`
  190 (+15 in MB2); `EnemyCatalogWindow.xaml.cs` 114 (+30 in MB2, +10 in MB6); `Game/Battle/BattlersView.cs` about 185
  after the battle turn presentation plan's phase 1 (+25 in MB7); `Data/Content/ContentWriter.ArtImports.cs` 111 (+10 in MB4).
- **Test files:** `ArtReferenceCheckTests` (239) → `ArtReferenceCheckBattlerTests` (new, MB1); `ContentWriterEnemyTests`
  (132) and `EnemyCatalogValidatorTests` (85) take their tests; `EnemyArtViewModelTests` (65); `ArtImageImportTests`
  (Glob first, sibling past 250). Globbed on 2026-11-14: no file named `*Gif*`, `*BattlerFrames*`, `*FrameScaler*`,
  `*StaThread*` in `src/` or `tests/`, and no STA helper in any test project.

## Out of scope (follow-ups, added to `docs/plans/backlog.md` by MB0)
- The party's battle animation (move right or left, idle) (decision 641).
- Attack or hit animations of the monster itself; a skill's own effect strip (skills name no effect today).
- Per-frame times (decision 632).
- The gnolls' stats, tactics, drops and encounters: content work (decision 640).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when a
slice is done and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the field
- [x] Needs the cinematics plan's CA0 (the GIFs in `art-src/gifs/`, renamed).
- [x] ADR 0047, animated battle sprites. Write it from the entry under "ADRs" below (decision 631).
- [x] **MB0. `ArtReferenceCheck`'s battle art into `ArtReferenceCheck.Battle.cs`** (Data, about 70 lines moved) →
  [slices/MB0.md](slices/MB0.md)
- [x] **MB1. An enemy's idle loop as content: `idleAnimation`, checked, left out while absent, its strip checked at
  startup** (Core + Data, about 200 lines) → [slices/MB1.md](slices/MB1.md)
- [x] **MB2. The Enemies window's idle boxes and the animated battle sprite preview** (Editor, about 210 lines) →
  [slices/MB2.md](slices/MB2.md)

### Phase 2: GIFs into strips
- [x] **MB3. A GIF read into whole frames and one frame time; the transparency finding recorded** (Editor, about 240
  lines) → [slices/MB3.md](slices/MB3.md)
- [x] **MB4. The frames scaled to a height, the background removed on demand, written as a strip; images wider than
  4096 px refused** (Editor + Data, about 220 lines) → [slices/MB4.md](slices/MB4.md)
- [x] **MB5. Import... takes a GIF as a battle sprite: the strip written, the idle boxes filled** (Editor, about 200
  lines) → [slices/MB5.md](slices/MB5.md)
- [x] **MB6. The GIF import's options dialog: battle height and background** (Editor, about 150 lines) →
  [slices/MB6.md](slices/MB6.md)

### Phase 3: in battle
- [x] **MB7. The battle loops each animated enemy's frames at the strip's size, one frame apart, held once fallen**
  (Game, about 230 lines) → [slices/MB7.md](slices/MB7.md)
- [x] **MB8. A GIF imported as an attack effect: scaled, square frames, its length reported; the effect frame time
  into Core** (Core + Game + Editor, about 150 lines) → [slices/MB8.md](slices/MB8.md)

## Decisions
- 631: a GIF becomes game art in the editor: on import, its frames composed whole become one PNG strip (frames side by
  side, one row) in `battlers/` or `effects/`; the game reads PNGs only (ADR 0009). The enemy record declares the loop:
  `"idleAnimation": { "frameCount": 8, "frameMilliseconds": 100 }` (ADR 0039's shape, reusing Core's `TileAnimation`),
  absent for a still sprite (ADR 0047).
- 632: one frame time per loop: the GIF's most common delay (the first of equals in frame order), a delay under 20 ms
  read as 100 ms (browsers' rule for 0 and 10 ms), filled into the box the developer can change; the import says when
  the GIF's delays differed.
- 633: the GIFs are renamed before import (the strip takes the GIF's name): `GnollArcher`, `HyenaWarlord` (the boss
  with the flail), `GnollAxeWarrior`, `GnollWarrior`, `GnollShaman`, `FireExplosion` (done in the cinematics plan's CA0).
- 634: the idle loop plays always for a living animated enemy (the party's turns, its own turn and action, while
  targeted); a fallen enemy holds the frame it fell on while it whitens and fades.
- 635: enemies' loops run one frame apart, by their place in the battle, so a trio of gnolls doesn't bob as one.
- 636: this plan doesn't change the step: a party member's stays 50 px over 0.2 s each way, linear (`BattlePacing`);
  enemies act in place since the battle turn presentation plan's TN1 (its decision 643), which runs before this plan
  (amended at that plan's approval, its decision 666).
- 637: the import scales each frame to a battle height in screen pixels (default 128, the goblin's drawn height under
  decision 100; editable per import, so a boss draws taller), aspect kept, resampled once (nearest neighbor for a whole
  factor up or down, so pixel art stays crisp; an area average otherwise). An animated battler draws at its frame's own
  size; a battler without a loop keeps decision 100's path, so every PNG already imported draws as today. An image wider
  than 4096 px (the widest texture the HiDef profile loads; the game sets no profile and MonoGame takes the highest
  supported) is refused at import, the message naming its width. A GIF of one frame is refused ("import a PNG").
- 638: the import can make the top-left pixel's color transparent in every frame (for a GIF drawn on a solid
  background), off by default, chosen in the import's dialog. MB3's step 0 found one such GIF: `GnollArcher.gif` is
  drawn on near-white (`FEFEFE`); the others are transparent around their figures (design, "What exists").
- 639: the explosion GIF becomes an attack effect strip: scaled to the import's height, each frame padded to a square
  (centered, transparent around), its GIF timing dropped; it plays at 0.08 s a frame (decision 57) until a record in
  `effect-strips.json` gives it its own frame time and hit frame (the battle turn presentation plan's EF1, its decision
  654, after this plan); the import reports the effect's length at that unlisted pace (frames x 0.08 s). The developer
  picks which enemy's attack plays it. (Amended at the battle turn presentation plan's approval, its decision 666.)
- 640: the gnoll enemies (stats, tactics, drops) are the developer's content in Database > Enemies... after MB5; a battle
  with one needs an encounter in `encounters.json` by hand, kept off the start area's random table (ADR 0017) or parked
  unrolled as the practice encounters are.
- 641: the party's battle animation is out of scope; nothing is built for it (a member's loop would be the same field on
  `PartyMemberDefinition` later).
- 642: `Data/Art/ArtReferenceCheck.cs` is split before it grows: its battle art (party and enemy battlers, enemy
  portraits, attack effects, the effect strips' check, and MB1's battler strips) moves into the partial
  `ArtReferenceCheck.Battle.cs` (MB0), the class becoming `public static partial`.

## Open design questions (developer decides)
None left: every question was answered at approval.

## ADRs
- [x] **ADR 0047: animated battle sprites** (decisions 631, 632, 637). Write it from this entry; the developer chose A.
  - **A. The editor converts a GIF into a PNG strip at import; the enemy record declares the loop (chosen).** Pros: the
    game stays PNG-only (ADR 0009); WPF decodes GIFs with no package; the scale, the background and the timing are
    chosen once, in the tool the enemy is set up in; the loop is set in the Enemies window's own session (one undo
    step); the field reuses ADR 0039's shape and `FrameAt`. Cons: GIF composing is ours to write and test; two enemies
    sharing a strip each set the loop (Duplicate copies it); a party member's loop would be the same field again.
  - **B. A per-image record, `battle-sprites.json`, as field sprites (ADR 0043).** Pros: the layout belongs to the
    image, shared by enemies and, later, party members. Cons: a new catalog with its loader, writer, draft setter and a
    nested window (or a combined undo step) in the Enemies window: about three slices more for one field.
  - **C. GIFs decoded at runtime.** Pros: no conversion step. Cons: needs a package or our own LZW decoder and frame
    composer in the game (Core can't, Data has no imaging); the editor must decode them too; GIF quirks at every start.
  - **D. Convert outside the editor (a script or an image tool), then today's PNG Import.** Pros: nothing new in the
    editor's import; any tool the developer likes. Cons: a manual step per GIF and per change of size; the frame
    count and time typed by hand from what the tool said; no repository script can decode GIFs without a package or
    Python (absent on this machine); the editor still needs MB1-MB2 for the loop.
