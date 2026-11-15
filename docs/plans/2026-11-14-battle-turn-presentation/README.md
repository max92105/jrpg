# Battle turn presentation: a turn that reads like the reference clip
Status: approved

Drafted on 2026-11-14 from the developer's request (cleaned up by the caller):

> Here is a full monster round [the reference GIF, an Octopath Traveler II monster turn]. I have new animations, more
> official but still not final, for slash and impact. I want our version of a turn to be as close as possible to this
> when it comes to animation and clarity: for example the faint glow that shows which monster is acting. We will not be
> moving monsters forward and backward anymore. I want the damage number to be exactly like that.

Approved on 2026-11-14 with the devil's advocate's findings applied (B1, C1-C7 and the nitpicks) and the developer's
answers (Q1-Q11, now decisions 643-666). What the developer liked most in the clip: on a monster's turn it **flashes
slightly instead of moving** (FL1, phase 1).

**Order across plans** (decision 662): phase 1 (TN0-NU1) runs right after
[`2026-11-14-cinematics`](../done/2026-11-14-cinematics/README.md) and before
[`2026-11-14-animated-monsters`](../done/2026-11-14-animated-monsters/README.md); phases 2-4 run after the animated-monsters
plan (EF7 imports the golden GIFs through its MB8; EF1 folds MB8's `EffectTiming`). The animated-monsters plan was
amended at approval (decision 666).

Builds on ADR 0013 (battle playback; amended for the wind-up), ADR 0009 (art as PNGs at runtime), ADR 0045 (Database
windows), ADR 0047 (an enemy's idle loop on its record), ADR 0036 (placeholder sounds), decision 57 (effect strips),
decision 100 (battler scaling), decision 101 (placeholder cues), decision 264 (another game's assets are reference only),
decision 423 (sounds at their beat).

- Numbers: decisions 643-666, ADRs 0048-0049, and an amendment of ADR 0013.
- Core: `Battle/EffectStripDefinition` and `ContentCatalog.EffectStrips` (EF1). No rule, event or save change.

## Goal
1. **A monster's turn plays like the clip:** the monster stays where it stands and flashes instead of moving; "Attack"
   pops in on a small dark plaque under the turn order with the action's own sound; the monster brightens twice with a
   soft spill; its slash sweeps in front of it, a golden streak crosses the target, and the hit lands about 0.7 s after
   the plaque: the number, the HP, the burst and the hit sound together. A party member's attack has the same shape,
   after its step forward.
2. **The damage number is the clip's:** at full size on the impact, at a party member's feet or just above an enemy's
   name tag, digits tight and stepping down, each hopping up in turn, gone at once after 0.57 s; white with a lavender
   lower part and a dark edge, from digit strips generated from the game's own OFL serif.
3. **The slash and the impact are content:** each effect strip has a record (its pace, the frame its hit lands on, the
   swing it plays first), edited in a Database window.
4. **The impact lights the scene:** a warm flash, nearby battlers washed white, the floor lit around the target, a
   lingering halo, and red and green fringes on the bright parts, all without a shader.

## Order, and why
1. **Phase 1, the turn (no new art; before the animated-monsters plan):** `BattleScene` split first (TN0, 290 lines),
   then the biggest wins: monsters stay put (TN1), the wind-up with Core executing at the impact (TN2), the plaque
   naming "Attack" (TN3) and the action's own sound at its start (TN4), the double flash (FL1), the number's motion and
   place (NU1).
2. **Phase 2, the look:** the plaque's shape (TN5), the digit strips generated (NU2) and drawn (NU3).
3. **Phase 3, the slash and the impact:** the strip records (EF1) and their startup check (EF2), the impact strip at its
   own pace and size (EF3) with its hit on the impact (EF4), the swing (EF5) and its sound (EF6), the Effect strips
   window (EE1-EE4), then the golden strips in the repository (EF7, through MB8's import and EE4's window).
4. **Phase 4, the impact's light:** the flash and wash (IL1), the floor glow and halo (IL2), the fringes (IL3).

## Guiding rules
- **Core decides nothing new:** Execute moves to the impact (ADR 0013 amended); Core only gains the strip records. Which
  strip shows where is presentation, in the Game.
- **One timeline, named:** every moment is a `BattlePacing` constant or a strip record's pace; nothing reads a clock;
  presenters are tested at the game's 1/60 s step (`StartMapScenarios.OneUpdate`) where they run through a stage.
- **No shader, no render target, no package:** light is additive sprite drawing over baked soft textures, and the
  fringe is the bright sprites drawn as three single-channel copies (ADR 0049).
- **No allocation per frame:** digits measured once, strip records looked up by file name in a frozen dictionary.
- **The editor never locks out on missing art** (decision 442): a record naming a missing strip is marked and saved.
- **Sizes:** tests at about 13 lines each (Editor: 10); a Game slice adding a view type at 1.6x its raw estimate;
  Editor slices about 200 lines.
- **Files near 300** (2026-11-14): `Game/Battle/BattleScene.cs` 290 (TN0 moves about 95 out first; TN4 +2, EF5 +3,
  IL1 +3, IL2 +1); `Game/Battle/ActionPlayback.cs` 292 (TN3 shrinks it about 20, EF3 +5); `Game/Ui/UiTextures.cs` 247,
  about 259 after the cinematics plan's `RingSegment` (TN5 +8 only if the rounded ends are needed, IL2 +8: about 275);
  `Game/Battle/BattlersView.cs` 162 (TN1 -2, FL1 +25, MB7 +25, IL1 +5, IL3 +15: about 230);
  `tests/.../ActionPlaybackTests.cs` 287 (TN3 removes about 35; EF3's tests go to `ActionPlaybackStripTests`);
  `tests/.../BattleSoundsTests.cs` 235 (TN4 moves 6 tests out); `tests/.../BattleTurnSequenceTests.cs` 221 (re-pins and
  EF5's constructor argument only; new tests in `BattleTurnSequenceWindUpTests`).
- **Test files named per slice** (Globbed 2026-11-14: none exists): `BattleTurnSequenceWindUpTests` (TN2),
  `ActionWindUpTests` (TN2, TN3), `ActionBannerTextTests` (TN3), `ActionSoundsTests` and `ActionWindUpSoundTests` (TN4,
  EF6), `ActorFlashTests`, `BattlerWhitenessTests`, `ActionWindUpFlashTests` (FL1; IL1 extends `BattlerWhitenessTests`),
  `DigitRowTests` and `EnemyTagViewTests` (NU1), `PlaqueLayoutTests` (TN5), `DigitStripTests` (NU3), `EffectStripDefinitionTests` (Core),
  `EffectStripCatalogValidatorTests`, `ContentCatalogLoaderEffectStripTests` (EF1), `ArtReferenceCheckEffectTests`
  (EF2), `ActionPlaybackStripTests` (EF3), `ActionWindUpEffectTests` (EF4, EF5), `ContentWriterEffectStripTests`,
  `CatalogDraftEffectStripTests` (EE1), `EffectStripViewModelTests` (EE2), `EffectStripCatalogViewModelTests` and
  `EffectStripEditingViewModelTests` (EE3),
  `ImpactLightTests` (IL1, IL2), `RadialGlowShapeTests` (IL2), `ColorFringeTests` (IL3). Existing ones extended:
  `FloatingNumberTests` (65), `BattleBannerTests` (35), `EffectStripTests` (28), `BattlerArtTests` (31),
  `PlaybackSoundQueueTests` (90), `BattleSceneSoundTests` (166), `CatalogProblemsTests`, `UiArtFilesTests`: each slice
  rereads its size and takes a sibling past 250.

## Out of scope (backlog lines at approval)
- **Projectiles** (decision 665): an effect travelling from the attacker to its target, and skills' and items' own
  swing and impact strips. The strip record's shape leaves room (a projectile field beside `SwingFile`, or fields on
  `SkillDefinition`).
- A baked soft glow (`GlowMask`, the silhouette's alpha blurred per frame) if FL1's cheap spill isn't enough.
- An animated preview of a strip in the Effect strips window.
- The target's hit reaction (the clip hides it), the HP bar's drain (unchanged), a camera shake (the clip has none).
- Per-frame times in a strip (decision 632 keeps one frame time per strip).
- Removing the unused `AttackEnemy.png` and `AttackPlayer.png` (decision 664).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when a
slice is done and write its deviations in the slice file. Tunables, the timeline, design overview and risks:
[design.md](design.md).

### Phase 1: the turn reads like the clip (after the cinematics plan, before the animated-monsters plan)
- [x] Needs the cinematics plan done.
- [x] **TN0. `BattleScene`'s outcome screens into `BattleOutcomeScreens`** (Game, about 110 lines moved) →
  [slices/TN0.md](slices/TN0.md)
- [x] **TN1. Enemies act in place; the party still steps** (Game, about 70 lines) → [slices/TN1.md](slices/TN1.md)
- [x] ADR 0013 amendment: an action executes at its impact, after a wind-up. Write it from the entry under "ADRs" below
  (decision 644).
- [x] **TN2. An action lands at its impact, after a 0.7 s wind-up** (Game, about 230 lines) →
  [slices/TN2.md](slices/TN2.md)
- [x] **TN3. The plaque names every action, "Attack" included, from its wind-up's start, popping in and out** (Game,
  about 170 lines) → [slices/TN3.md](slices/TN3.md)
- [x] **TN4. An action's own sound at its wind-up's start, its hit at the impact** (Game, about 200 lines) →
  [slices/TN4.md](slices/TN4.md)
- [x] **FL1. The actor brightens twice with a soft spill instead of moving: `ActorFlash`, `BattlerWhiteness`** (Game,
  about 250 lines) → [slices/FL1.md](slices/FL1.md)
- [x] **NU1. The damage number at a member's feet or above an enemy's tag: digits stepping down, hopping in turn, gone
  at 0.57 s** (Game, about 260 lines) → [slices/NU1.md](slices/NU1.md)

### Phase 2: the look (after the animated-monsters plan)
- [x] **TN5. The plaque's look: a small dark plaque with a hairline, hugging the word** (Game, about 150 lines) →
  [slices/TN5.md](slices/TN5.md)
- [x] **NU2. The digit strips generated from Cormorant Garamond by a script in `art-src/`** (art + script, about 150
  lines) → [slices/NU2.md](slices/NU2.md)
- [x] **NU3. The digits drawn from the strips; a critical from its own bigger strip** (Game, about 200 lines) →
  [slices/NU3.md](slices/NU3.md)

### Phase 3: the slash and the impact
- [x] ADR 0048, battle effect strips as records. Write it from the entry under "ADRs" below (decisions 654, 657).
- [x] **EF1. Effect strip records: `effect-strips.json` (pace, hit frame, swing), loaded and checked** (Core + Data,
  about 210 lines) → [slices/EF1.md](slices/EF1.md)
- [x] **EF2. The startup check: each listed strip and swing in `effects/`, its hit frame within its frames** (Data,
  about 140 lines) → [slices/EF2.md](slices/EF2.md)
- [x] **EF3. The impact strip at its own pace and size, centered on the target, mirrored for the party** (Game, about
  190 lines) → [slices/EF3.md](slices/EF3.md)
- [x] **EF4. The impact strip's hit frame lands on the impact** (Game, about 200 lines) → [slices/EF4.md](slices/EF4.md)
- [x] **EF5. The swing its impact strip names sweeps in front of the attacker** (Game, about 120 lines raw, about 190
  at the 1.6x) → [slices/EF5.md](slices/EF5.md)
- [ ] **EF6. The swing's placeholder sound from freesound, at the swing's start** (content + Game, about 110 lines) →
  [slices/EF6.md](slices/EF6.md)
- [x] **EE1. `effect-strips.json` saved from the editor: written sorted, defaults left out,
  `CatalogProblems.OfEffectStrips`, `CatalogDraft.SetEffectStrips`** (Data + Editor, about 200 lines) →
  [slices/EE1.md](slices/EE1.md)
- [x] **EE2. One strip as typed: pace and hit frame boxes, the swing, its length, its problem** (Editor, about 200
  lines) → [slices/EE2.md](slices/EE2.md)
- [x] **EE3. The Effect strips window's rules: every effects image, unlisted or not, reset, one undo step** (Editor,
  about 210 lines) → [slices/EE3.md](slices/EE3.md)
- [x] **EE4. The Effect strips window and Database > Effect strips...** (Editor, about 170 lines) →
  [slices/EE4.md](slices/EE4.md)
- [ ] **EF7. The golden slash and impact in the repository** (content + hand import, about 60 lines) →
  [slices/EF7.md](slices/EF7.md)

### Phase 4: the impact's light
- [x] ADR 0049, battle screen effects as additive sprite drawing. Write it from the entry under "ADRs" below
  (decisions 660, 661). Written: [0049](../../decisions/0049-battle-screen-effects-as-additive-sprites.md).
- [x] **IL1. The impact's warm flash and the white wash on nearby battlers** (Game, about 140 lines raw, about 220 at
  the 1.6x) → [slices/IL1.md](slices/IL1.md)
- [ ] **IL2. The floor glow around the target and its lingering halo** (Game, about 200 lines) →
  [slices/IL2.md](slices/IL2.md)
- [ ] **IL3. Red and green fringes on the impact's bright parts** (Game, about 180 lines) → [slices/IL3.md](slices/IL3.md)

## Decisions
- 643: monsters act in place (the developer's answer). An enemy's turn keeps its stage times, so it pauses 0.2 s after
  the turn order switches to it and 0.2 s after its action, in place; a party member steps 50 px out and back over 0.2 s
  as today. Supersedes decision 636's "every actor" for enemies.
- 644: an action lands at its impact (ADR 0013 amended: its option A's "Execute at the moment of impact"): an Attack, a
  skill or an item winds up 0.7 s before Core executes it; Defend, Move and Run don't wind up. HP, statuses, resources
  and falls change on screen at the impact through the existing stage-change refresh. An enemy's tactics still choose
  when its pause ends.
- 645: the impact hold goes from 1.0 s to 0.85 s, so a monster's turn takes about 1.95 s (0.2 + 0.7 + 0.85 + 0.2), the
  clip's about 1.9 s (placeholder).
- 646: the plaque shows every action's name from the start of its wind-up, an Attack as "Attack" (`battle_attack`);
  Run shows nothing. It pops in and out (no fade) and stays 1.5 s. Skills, items, Defend and Move use the same plaque
  (Q10). Its look starts as a plain dark rectangle with a hairline, hugging the text at 24 px; rounded ends only if the
  capture comparison shows them missing (TN5).
- 647: an action's sounds have two moments: its own sound at its wind-up's start (a skill's sound file, the item-use or
  Defend sound), its impact sound at the impact (hit, critical, heal, miss, block...). One type, `ActionSounds`, says
  both, read by the wind-up and by `BattleSounds`; content gains per-action opening and impact sound files later (the
  developer will add them), each filling one of the two moments (Q8, C6).
- 648: the actor flash (the developer's favorite: a monster flashes slightly instead of moving): the actor, party member
  or enemy, brightens twice in its wind-up, at 0.10 s for 33 ms and at 0.20 s for 100 ms, white laid at 0.6 over the
  sprite so its detail shows through (the fallen's strength); both blinks spill softly past the outline (the white
  silhouette drawn 8 times around it at low strength). Not a lasting aura (C5).
- 649: one pure rule, `BattlerWhiteness`, says how white a battler draws: the fallen's white, the actor's blink and the
  impact's wash, the strongest winning (C7).
- 650: the number's motion, pinned to the clip's sampled frames (C3): at full size on the impact, its digits tight,
  each 6 px lower than the one before; digit k rises 36 px from 0.067 + k x 0.033 s, stays near the top and lands by
  0.2 s (later digits of a longer number keep a 0.033 s top and land after: the 2-digit clip can't say); it vanishes at
  once 0.57 s after it appeared. No pop, no rise, no fade.
- 651: where the number sits (C4, Q7): centered on its target; a party member's row at the feet (the clip); an enemy's
  row with its bottom just above the top of the enemy's name tag (`EnemyTagView`), so the two never overlap.
- 652: every floating number moves this way (damage, heal, Poison, backfire, tick, life stolen) in its color; a critical
  is 1.4x bigger and red-gold (`UiTheme.Battle`, placeholder; Q3); "WEAK" or "RESISTED" stays beside the row, still,
  and goes with it; a multi-hit's numbers stack 48 px higher.
- 653: the digits are two strips generated from `content/art/fonts/titles/CormorantGaramond-SemiBold.ttf` (OFL-1.1)
  by `art-src/digits/New-DigitStrips.ps1` (PowerShell and System.Drawing, a one-off art tool the game never uses):
  slanted, heavy, white fading to a pale lavender-grey below, a thin dark edge; the clip's look, not its glyphs
  (decision 264); a normal strip and a critical strip 1.4x bigger, listed in `art-sources.json` under the Cormorant
  source's licence. Generated in their own slice (NU2) before the game draws them (NU3); NU1's body font until then
  (Q4).
- 654: an effect strip's pace is a record (ADR 0048): `content/effect-strips.json` gives a strip by file name its frame
  time, the frame its hit lands on and the swing it plays first; a strip it doesn't list plays at 80 ms a frame (decision
  57's value) with its hit on frame 0 and no swing.
- 655: an effect strip draws at its frame's own size (MB8's import height chose it; decision 57's fixed 96 px goes): an
  impact centered on the target's drawn sprite, a swing ahead of the attacker. Effect art faces right, as an enemy
  attacking the party; a party member's is drawn mirrored.
- 656: the impact strip's hit frame shows at the impact: the strip starts its lead (hit frame x frame time) before it,
  on the targets a damaging action aims at, and goes on after the impact only where damage landed (a miss cuts it at
  its hit frame). A lead longer than the wind-up loses its first frames, so the hit stays on the impact.
- 657: the swing is named by the attacker's impact strip's record (`swingFile`, C2), not per attacker, and plays for
  the Attack command from 0.47 s into the wind-up. Damaging skills and items play the impact strip only, for now (Q2).
- 658: the swing's sound is a freesound placeholder (ADR 0036's rules), found and fetched in EF6, played at the swing's
  start (Q9).
- 659: the strip records get an editor now (Q11): Database > Effect strips..., ADR 0045's shape. It lists every image
  in `effects/` (an unlisted one shows the unlisted pace) and the records whose image is missing; editing an image's
  boxes lists it, Reset to unlisted removes its record; one undo step per session; no undo refusal (nothing refers to a
  record by id).
- 660: the impact's light plays where damage landed, in one warm color for every impact (Q5): the battlefield flashes
  warm (additive, under the HUD) and battlers within 400 px are washed white, both near full for about 0.1 s and gone by
  0.2 s; the floor around the target is lit no longer; a warm halo on the target rises as the flash ends, holds to about
  0.43 s and is gone by 0.5 s (the clip's, C3). The floor glow is drawn by `ImpactLightView` between the backdrop and
  the battlers, called from `BattleScene` (C7).
- 661: the color fringes are local (C1, Q6): while they last, the impact strip's frame and the white silhouettes are drawn
  as three additive single-channel copies (red shifted left, green right, blue in place), before the numbers; about 6
  virtual px apart at the peak, gone by about 0.167 s. No host, flow, render-target or shader change (ADR 0049).
- 662: order (Q1 = b): phase 1 runs after the cinematics plan and before the animated-monsters plan; phases 2-4 after
  the animated-monsters plan (EF1 folds MB8's `EffectTiming`; EF7 uses MB8's import).
- 663: `Game/Battle/BattleScene.cs` (290 lines) gives up its outcome screens first (TN0), so the plan's additions fit.
- 664: `AttackEnemy.png` and `AttackPlayer.png` stay in `content/art/effects/`, unused after EF7 (the new art isn't
  final); a cleanup removes them with their `art-sources.json` lines (backlog).
- 665: projectiles and skills' own swing and impact strips are later (Q2): out of scope, with a backlog line; the
  record's shape leaves room for them.
- 666: the animated-monsters plan was amended at approval: decision 636 (only the party steps), decision 639 (an effect's
  pace from its record once EF1 lands), goal 3 and MB7 (the loop plays in place; the flash uses the frame), MB8 (its
  `EffectTiming` folds into EF1's record), and a line pointing to this plan.

## Open design questions (developer decides)
None left: every question was answered at approval.

## ADRs
- [x] **ADR 0013 amendment: an action executes at its impact, after a wind-up** (decision 644). Write it from this entry.
  Under Decision: "From the battle turn presentation plan (2026-11-14): between the command and `Execute` the turn holds
  a wind-up stage (0.7 s for an Attack, a skill or an item), shown from the command alone (its name, its sound, the
  actor's blinks, the swing, the impact strip's lead); `Execute` runs when it ends, at the impact, so the HUD still reads
  live state at each stage change and shows the outcome exactly at the hit." Consequences: the wind-up can't know a miss
  before it lands (the impact strip's lead may play on a target the hit misses); a skill's damage is read from its
  definition; an enemy's tactics choose 0.7 s before `Execute`, with the same draws in the same order. The rejected
  alternative, kept in a line: executing at the start with a wind-up beat first would put Core 0.7 s ahead of every
  live-state reader (panels, tags, status marks, pips, the fallen's whitening) and re-pin about 40 timing tests.
- [x] **ADR 0048: battle effect strips as records** (decisions 654, 657). Write it from this entry; the developer chose
  A.
  - **A. A record per strip image, `effect-strips.json` (frame time, hit frame, swing file) (chosen).** Pros: facts
    about the art live with the art, once for a strip 13 records name today; another strip (the explosion) lines up its
    own hit; the swing belongs with its impact; edited in one Database window. Cons: a catalog with its loader, check,
    writer and window.
  - **B. Fields on each attacker, ADR 0047's shape (the idle loop on the enemy record).** ADR 0047 put the idle loop on
    the enemy because a loop belongs to that enemy's sprite, one owner; an effect strip differs: one strip is shared by
    many enemies and party members, so its pace on each attacker would be copied 13 times and drift. Cons as said; pros:
    no new catalog.
  - **C. Game constants per slot (swing pace, impact pace, impact lead).** Pros: no data. Cons: art facts in code; every
    impact strip shares one hit frame; an art change is a code change.
- [x] **ADR 0049: battle screen effects as additive sprite drawing** (decisions 660, 661). Write it from this entry; the
  developer chose A.
  - **A. Additive sprite drawing (chosen):** the light (flash, wash, floor glow, halo) drawn in short additive batches
    over textures baked once; the fringe as the bright sprites (the impact strip's frame, the white silhouettes) drawn
    as three additive single-channel copies shifted apart, so the HUD and the numbers stay clean. Pros: no shader, no
    package, no content pipeline (ADR 0009 holds), no render target set mid-frame (the virtual screen discards its
    contents when set again, `JrpgGame.FreezeFrameIfWanted`); local to the battle. Cons: the fringe touches only what
    is redrawn, not the background's edges; F12 captures show it (it's in the frame).
  - **B. A post-process at the host:** the finished frame drawn three times, tinted and shifted, when the host copies it
    to the window, the shift passed through the flow. Pros: a true whole-frame aberration in a few lines. Cons: it
    fringes the HUD and the numbers; a scene-to-flow-to-host channel for one effect; captures miss it.
  - **C. An HLSL effect.** Pros: real bloom and aberration. Cons: the MonoGame content pipeline (an `.mgcb`, the effect
    compiler as a dotnet tool package, a build step), against ADR 0009's runtime PNGs; desktop GL shader quirks.
