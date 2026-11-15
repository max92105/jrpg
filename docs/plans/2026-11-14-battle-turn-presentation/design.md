# Battle turn presentation: design

## Tunables and placeholders

Every size is on the 1920 x 1080 virtual screen (the clip's 640 x 360 times 3). Times were read from the clip's 33 ms
frames and are placeholders to tune by eye.

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Enemy pause | `Game/Battle/BattlePacing.Step` (exists) | 0.2 s | An enemy's StepForward and StepBack stages, spent in place (decision 643). |
| Party step | `BattlePacing.StepDistance`, `Step` (exist) | 50 px, 0.2 s | Unchanged. |
| Wind-up | `BattlePacing.WindUp` (TN2) | 0.7 s | From the plaque to the impact, for an Attack, a skill or an item (decision 644). |
| Impact hold | `BattlePacing.ImpactHold` (exists, 1.0 s) | 0.85 s | From the impact to the step back (decision 645). |
| Plaque time | `Game/Battle/BattleBanner` | shown 1.5 s, no fade | The clip's plaque leaves about 1.5 s after it came (decision 646). |
| Plaque look | `Game/Battle/BattleBannerView`, `Ui/UiTheme.Battle` | 24 px text, padding 14 x 6 px, near-black navy at 85 %, a 1 px hairline top and bottom, top at y 190 | The clip's plaque: about 100 x 36 around "Attack". |
| First blink | `Game/Battle/ActorFlash` | from 0.10 s for 0.033 s | Decision 648. |
| Second blink | `ActorFlash` | from 0.20 s for 0.10 s, its last 0.03 s fading | Decision 648. |
| Blink strength | `Game/Battle/BattlerWhiteness` | white at 0.6 over the sprite | The fallen's strength, so the sprite's detail shows (decision 648). |
| Blink spill | `Game/Battle/BattlersView` | the white silhouette 8 times around the sprite, 6 px out, each at 0.12 x the blink | The soft spill past the outline (decision 648). |
| Swing start | `BattlePacing.SwingStart` (EF5) | 0.47 s into the wind-up | The clip's slash. |
| Swing reach | `Game/Battle/SwingView` | 60 px toward the other side | How far ahead of the attacker's center the swing's frame centers. |
| Unlisted strip pace | Core `EffectStripDefinition.UnlistedFrameTime` (EF1; MB8's `EffectTiming.FrameTime`) | 80 ms a frame, hit frame 0, no swing | Decision 57's value (decision 654). |
| Golden slash pace | `content/effect-strips.json` (EF7) | 17 ms a frame (12 frames: about 0.2 s) | The clip's slash, its tail over the hit. |
| Golden impact pace | `content/effect-strips.json` (EF7) | 80 ms a frame, hit frame 2 (the burst), swing `GoldenSlashAttack.png` | Lead 0.16 s (the streak), sparks to 0.48 s after the hit. |
| Import heights | MB8's dialog, EF7's hand step | slash 340 px, impact 480 px | MB4 refuses an image over 4096 px wide: 12 x 340 = 4080, 8 x 480 = 3840. |
| Number font size | `Game/Battle/FloatingNumbersView` (NU1) | 54 px body font | Its digits about 39 px tall, until the strips exist. |
| Number at a member's feet | `FloatingNumber.RowBottomOf` (NU1) | the row's bottom 10 px below the feet line | "At the feet, overlapping the lower body slightly" (decision 651). |
| Number above an enemy's tag | `FloatingNumber.RowBottomOf` (NU1) | the row's bottom 4 px above the tag's top | The tag's top from `EnemyTagView` (decision 651). |
| Digit stagger | `Game/Battle/FloatingNumber` | 6 px lower per digit | The clip's descending step. |
| Digit spacing | `FloatingNumber` | -2 px | Digits tight together. |
| Hop | `FloatingNumber` | digit k rises from 0.067 + k x 0.033 s over 0.033 s, 36 px; holds near the top; falls over 0.033 s, landing at 0.2 s (later digits: 0.033 s at the top, then fall) | Pinned to the clip's frames #80-#84 (decision 650). |
| Number life | `FloatingNumber` | 0.57 s, then gone at once | Decision 650. |
| Stack spacing | `FloatingNumber.StackSpacing` (exists, 36 px) | 48 px | A multi-hit's numbers, clear of the hop. |
| Critical | `FloatingNumber`, `UiTheme.FloatingCritical` | 1.4x (its own strip from NU3), red-gold | Decision 652. |
| Digit strips | `content/art/fonts/DamageDigits.png`, `CriticalDigits.png` (NU2) | 10 cells of 40 x 52 px; 10 of 56 x 73 px | Spec in NU2. |
| Scene flash | `Game/Battle/ImpactLight`, `UiTheme.ImpactWarmth` | additive warm orange-white, 45 % to 0.1 s, 0 at 0.2 s | Decision 660. |
| Wash | `ImpactLight` | 0.6 to 0.1 s, 0 at 0.2 s; battlers within 400 px of the target, falling off linearly | The clip's bloom on nearby sprites. |
| Floor glow | `ImpactLight`, `Ui/RadialGlowShape` | a 600 x 200 px ellipse at the target's feet, 80 % to 0.1 s, 0 at 0.2 s | The lit floor. |
| Target halo | `ImpactLight` | a 280 px disc on the target's center, 0 at 0.1 s rising to 50 % at 0.2 s, held to 0.43 s, 0 at 0.5 s | The warm halo around the crescent glow (frames #83-#90). |
| Fringe | `Game/Battle/ColorFringe` | 6 virtual px at the impact, easing to 0 at 0.167 s; red left, green right | Decision 661. |

## The turn's timeline (an enemy's Attack, the placeholders above)

```
  s from the turn's start
  0.00  StepForward  the turn order's big diamond switches; the enemy pauses in place (a party member steps out)
  0.20  WindingUp    the plaque pops in: "Attack"; the action's own sound      (wind-up 0.00)
  0.30                 first blink, 33 ms                                       (0.10)
  0.40                 second blink, 100 ms                                     (0.20)
  0.67                 the swing starts in front of the attacker, 12 x 17 ms, with its sound (0.47)
  0.74                 the impact strip's lead starts on the aimed target: 2 x 80 ms, the streak (0.54)
  0.90  Holding      Core executes. The impact: number, HP, hit sound, burst frame, light, fringes (0.70)
  0.97                 the first digit rises; the second at 1.00
  1.00                 flash, wash and floor glow start fading; fringes gone at 1.07
  1.10                 the digits have landed; flash, wash, floor glow gone; the halo at its height
  1.40                 halo gone; the number vanishes at 1.47
  1.70                 the plaque vanishes
  1.75  StepBack     the enemy pauses in place (a party member steps back)
  1.95  the next actor's StepForward
```

## Design overview

### What exists
- **Game** `Battle/BattleTurnSequence` runs `TurnStage`s (StepForward, AwaitingCommand, Holding, StepBack, Over). A party
  member's `Submit`, or an enemy's tactics at the end of StepForward, calls `BattleState.Execute` at once and holds for
  `ActionPlayback.Duration`; `StepShare` steps any actor out, which `BattlersView.SteppedSpot` turns into 50 px toward
  the other side (enemies right, the party left). Five test sites build the sequence.
- `Battle/ActionPlayback` (292 lines) reads the events into beats (a 0 s Banner, Impact 1.0 s, Defeat, Skip, Backfire,
  Tick, Phase); numbers, strips and cues show from the impact at its `Elapsed` 0; `BannerTextFor` gives no text for an
  Attack. `BattleSounds` times each sound at its beat (a skill's, the item-use and the Defend sound at 0 from
  `ActionStarted`; the hit sounds at the impact); `PlaybackSoundQueue` asks them as `Elapsed` reaches them.
- `BattleScene` (290 lines) shows the banner when a new playback appears and refreshes the panels, tags and spots at
  each stage change (ADR 0013).
- `BattleBanner` fades in 0.15 s, holds 1.2 s, fades out 0.3 s; `BattleBannerView` draws a `FramedWindowView` at least
  300 px wide with 28 px text, its top at y 190.
- `FloatingNumber` pops from 60 % in 0.18 s, rises 50 px over 1.6 s and fades its second half; `FloatingNumbersView`
  draws it at 48 px through `OutlinedFont`, above the sprite's top (`BattlersView.TopOf`, which `CommandPanelView` also
  reads), a critical 1.4x and gold, a stack 36 px higher, "WEAK" or "RESISTED" beside it.
- `EnemyTagView` places each enemy's name tag at its spot's left, its bottom 38 px below the spot's bottom.
- `ImpactEffectsView` draws the attacker's `AttackEffectFile` strip (every damage, decision 101) at 96 px, 30 px under
  the target's top, at `EffectStrip`'s fixed 0.08 s a frame (MB8 moves that value to Core's `Battle/EffectTiming`), and
  placeholder cues in the same box.
- `BattlersView` lights the fallen with `UiTextures.WhiteSilhouette` at 0.6. Nothing draws additively.
- **Core** `EnemyDefinition` and `PartyMemberDefinition` each name one `AttackEffectFile`; `ContentCatalog`'s newer
  catalogs (`FieldSprites`, `Cinematics`) are non-required with empty defaults.
- **Data** `Art/ArtReferenceCheck` (its battle part in `ArtReferenceCheck.Battle.cs` after MB0) checks attack effects by
  exact name and as rows of square frames; `ContentCatalogLoader` reads each catalog through `ReadValidated`;
  `ContentWriter.Catalogs` lists the files the editor writes.
- **Editor** the Database windows (ADR 0045) and the Character sprites window (records by image file, every image
  listed, `FieldSpriteCatalogViewModel`) are the models for the Effect strips window.
- **Art** `art-src/gifs/GoldenSlashAttack.gif` (12 frames, 362 x 362, 80 ms: a golden crescent sweeping right) and
  `GoldenSlashImpact.gif` (8 frames, 313 x 627, 80 ms: frames 0-1 a diagonal streak, 2 the burst, 3-4 the crescent
  glow, 5-7 sparks and embers). `content/art/fonts/titles/CormorantGaramond-SemiBold.ttf` (OFL-1.1, the
  `cormorant-garamond` source of `art-sources.json`).

### Core changes
- EF1: `Battle/EffectStripDefinition` (public sealed record: `File`, `FrameMilliseconds`, `HitFrame` = 0, `SwingFile` =
  null; `FrameTime`, `HitLead`; `static TimeSpan UnlistedFrameTime`; `static EffectStripDefinition Of(string file,
  IReadOnlyDictionary<string, EffectStripDefinition> listed)`: the listed record, else the unlisted pace with no swing).
  MB8's `Battle/EffectTiming` folds into it. `Content/ContentCatalog.EffectStrips` (by file, non-required, empty
  default).
- No rule, event or save change; `EnemyDefinition` and `PartyMemberDefinition` unchanged.

### Content format
```json
[
  {
    "file": "GoldenSlashAttack.png",
    "frameMilliseconds": 17
  },
  {
    "file": "GoldenSlashImpact.png",
    "frameMilliseconds": 80,
    "hitFrame": 2,
    "swingFile": "GoldenSlashAttack.png"
  }
]
```
`content/effect-strips.json`, sorted by file, `hitFrame` left out at 0 and `swingFile` while null (the writer's layout
from EE1).

### Structure and folder dependencies

| Type | Project/folder | Responsibility |
|---|---|---|
| `EffectStripDefinition` (EF1) | Core `Battle/` | One effect strip's pace: frame time, the frame its hit lands on, the swing it plays first. |
| `EffectStripCatalogValidator` (EF1) | Data `Battle/` | Checks `effect-strips.json`: names, uniqueness, pace bounds. |
| `BattleOutcomeScreens` (TN0) | Game `Battle/` | Which screen a finished battle shows (victory, escape or defeat), with its music and sounds. |
| `ActionWindUp` (TN2) | Game `Battle/` | What one action shows before it lands, at the time reached: actor, name, sounds, blinks, aimed strip, swing. |
| `ActionBannerText` (TN3) | Game `Battle/` | The words the plaque shows for an action, from its actor and command. |
| `ActionSounds` (TN4) | Game `Battle/` | Which sound an action makes at its wind-up's start and which at its impact. |
| `ISoundTimeline` (TN4) | Game `Battle/` | Sounds due at times along their own clock (the wind-up's and the playback's). |
| `ActorFlash` (FL1) | Game `Battle/` | The acting battler's double blink: its strength at a time into the wind-up. |
| `BattlerWhiteness` (FL1) | Game `Battle/` | How white a battler draws: the fallen's, the blink's or the wash's, the strongest. |
| `DigitRow` (NU1) | Game `Battle/` | Where each digit of a number sits along its row, from the digits' advances. |
| `PlaqueLayout` (TN5) | Game `Ui/` | Where a plaque sits around a measured text. |
| `DigitStrip` (NU3) | Game `Battle/` | Each digit's columns in a digit strip image, measured once from its pixels. |
| `SwingView` (EF5) | Game `Battle/` | Draws an attack's swing in front of its attacker. |
| `EffectStripViewModel` (EE2) | Editor `Battle/` | One strip's record as typed: pace, hit frame, swing, length, problem. |
| `EffectStripCatalogViewModel` (EE3) | Editor `Battle/` | Every effects image with its record or the unlisted pace; reset; one undo step. |
| `EffectStripEditingViewModel` (EE3) | Editor `Battle/` | Starts the window's session from the draft and applies it as one step. |
| `EffectStripCatalogWindow` (EE4) | Editor `Battle/` | The Effect strips window. |
| `ImpactLight`, `ImpactLightLook` (IL1) | Game `Battle/` | How the impact's light looks at a time since it (flash, wash, floor glow, halo), and that look. |
| `ImpactLightView` (IL1) | Game `Battle/` | Draws the impact's floor glow under the battlers and its flash and halo over them. |
| `RadialGlowShape` (IL2) | Game `Ui/` | A soft disc's pixels, baked once. |
| `ColorFringe` (IL3) | Game `Battle/` | How far the bright parts' red and green split at a time since the impact. |
| `DrawnSprite` (IL3) | Game `Battle/` | A texture's part as drawn: where, which part, flipped or not. |
| `ChannelSplit` (IL3) | Game `Battle/` | Draws a texture as three additive single-channel copies, red and green shifted apart. |

| From | To | Why | New? |
|---|---|---|---|
| Core `Content` | Core `Battle` | `ContentCatalog.EffectStrips` | no (enemies) |
| Data `Content` | Data `Battle` | the loader calls the strip validator; `CatalogProblems.OfEffectStrips` | no |
| Data `Art` | Core `Battle` | the startup check reads the strips | no |
| Game `Battle` | Game `Ui` | shapes, glow textures, theme | no |
| Game `Battle` | Game `Rendering` | `VirtualScreen.BeginLightBatch` (IL1) | no |
| Editor `Battle` | Editor `Catalogs` | `TypedNumber`, `CatalogDraft.SetEffectStrips` | no |
| Editor `Shell` | Editor `Battle` | Database > Effect strips... | no |

`Ui` gains no edge to `Battle`. Nothing new reaches `Flow` or the host.

### Key decisions
- **Execute at the impact** (decision 644, ADR 0013 amended): a `WindingUp` stage between the command and `Execute`.
  `BattleTurnSequence.Submit` and the enemy's tactics start an `ActionWindUp` from the command; when it ends, `Act`
  executes and Holding starts, so `BattleScene.ShowStage` refreshes the HUD exactly at the impact. `ActionPlayback`
  keeps its beats; its `Elapsed` 0 is still the impact.
- **The wind-up decides from the command:** its length (an Attack, a skill or an item: 0.7 s; Defend, Move, Run: 0),
  its name (`ActionBannerText`), its opening sound (`ActionSounds`), its swing (an Attack whose impact strip names one),
  and the targets its impact strip aims at (an Attack's target; a skill's or item's whose effect is damage). After the
  impact the playback's strips (damage landed) carry on.
- **Two clocks, two sound timelines:** `ActionWindUp.Elapsed` from the plaque, `ActionPlayback.Elapsed` from the impact;
  both keep running after their stage, so a swing's tail plays over the impact and the last numbers finish during the
  next step. Both implement `ISoundTimeline`, each followed by its own `PlaybackSoundQueue` in `BattleScene`.
- **Strip records, drawing in the Game** (decisions 654-657, ADR 0048): `Game/Battle/EffectStrip` takes the image size
  and the record; `FrameAtImpactTime(sinceImpact)` is `FrameAt(HitLead + sinceImpact)`, so the hit frame shows on the
  impact whatever the lead.
- **Light and fringe as additive sprites** (ADR 0049): soft textures baked once, drawn in short additive batches only
  while the light lasts; the fringe redraws the bright parts as single-channel copies, before the numbers.

### Extension points
- Projectiles (decision 665): a projectile field on `EffectStripDefinition` beside `SwingFile` (a strip flying from the
  attacker to the target over a travel time before the impact strip), read by `ActionWindUp` as the swing is.
- Skills' and items' own strips: optional fields on `SkillDefinition` and `ItemDefinition`, read by `ActionWindUp.Of`
  before the attacker's.
- Per-action sounds (decision 647): an opening and an impact sound file on a skill or item, read by `ActionSounds`
  before today's defaults (the impact one takes the command then).
- A light color per strip: a field on `EffectStripDefinition`, read by `ImpactLight`'s caller (one color for now, Q5).
- Per-frame times: `EffectStripDefinition` gaining a list (decision 632 keeps one time).

### Replacement mapping

| Existing | Replaced by | When |
|---|---|---|
| `BattleScene`'s outcome fields and methods, `BattleMusic` | `BattleOutcomeScreens` | TN0 |
| Enemies stepping right (`BattlersView.SteppedSpot`, `StepShare`) | enemies in place | TN1 |
| `Execute` at `Submit` or at an enemy's StepForward end | `Execute` when the `WindingUp` stage ends | TN2 |
| `ActionPlayback.BannerText`, `BannerTextFor`, `PlaybackBeatKind.Banner`, `BattleScene.ShowPlayback` | `ActionBannerText.For`, `ActionWindUp.BannerText`, the scene showing it when a wind-up starts | TN3 |
| `BattleBanner`'s fades (0.15 / 1.2 / 0.3 s) | pops in, 1.5 s, pops out | TN3 |
| `BattleSounds`' `ActionStarted` sounds and `ImpactFileOf` | `ActionSounds.OpeningOf` (the wind-up) and `ActionSounds.ImpactOf` (read by `BattleSounds`) | TN4 |
| `BattlersView`'s fallen-only whitening | `BattlerWhiteness` (fallen, blink; wash in IL1) | FL1 |
| `FloatingNumber.At`, `FloatingNumberLook` (pop, rise, fade) | `DigitOffsetAt`, `IsShownAt`, `RowBottomOf` | NU1 |
| Numbers above the sprite's top (`TopOf`) | at a member's feet, above an enemy's tag | NU1 |
| `BattleBannerView`'s `FramedWindowView` (300 px minimum, 28 px) | the plain plaque (24 px, hugging the text) | TN5 |
| The outlined body font's digits | the digit strips | NU3 |
| `EffectStrip`'s one frame time (decision 57; MB8's `EffectTiming.FrameTime`) | `EffectStripDefinition` per strip, MB8's value its unlisted pace | EF1, EF3 |
| The 96 px effect box 30 px under the top | the strip's own size, centered on the drawn sprite (`BattlersView.AreaOf`, beside `TopOf`) | EF3 |
| The impact strip from the impact on | from its lead, its hit frame on the impact | EF4 |
| The hand-edited strip pace | the Effect strips window | EE1-EE4 |

### Assumptions (the clip doesn't show them)
- Heal, Poison, backfire, tick and life-stolen numbers move as damage numbers do, in their colors (decision 652).
- "Miss", "Blocked", status names and the other placeholder cues stay as today (decision 101), placed on the target's
  drawn center once the 96 px box goes (EF3).
- A skill's resource cost shows when it lands (the pips change at the impact).
- How a number of three or more digits lands (decision 650): later digits keep a short top and land after the second.
- The target's own reaction and the HP bar's drain stay as today.

## Risks
- **Execute later:** tactics choose 0.7 s before `Execute`; no other actor acts between, so the battle draws the same
  numbers in the same order (determinism kept, the seeded tests' outcomes unchanged).
- **A streak before a miss:** the impact strip's two lead frames play on an aimed target the hit misses, then stop
  (decision 656). Checked at EF4's hand check.
- **Test churn:** TN1 re-pins 2 sequence tests, TN2 about 8, TN3 moves 5 banner tests, TN4 moves 6 sound tests, NU1
  replaces 3 number tests, EF3 and EF5 re-pin the 2 tests that pin a repository member's strip (they read it from the
  record after, so EF7 passes), EF1 the 3 `EffectStripTests` call sites, EF5 the 5 sequence constructions.
- **Before the animated-monsters plan:** phase 1 whitens whole images; MB7 (amended) passes the frame's rectangle to the
  blink and its spill copies as to the fallen's.
- **Texture width:** MB4 refuses an image over 4096 px: the 12-frame slash imports at 341 px high at most (EF7 says
  340).
- **Old-style figures:** GDI+ can't pick Cormorant's lining figures; NU2's script scales each digit's ink to one height
  and one baseline, whatever the font's default figures are.
- **Additive drawing, first use:** each light or fringe draw ends and begins the batch, only while it lasts (at most
  0.5 s a hit).
- **The fringe is local:** the background's edges don't split, unlike the clip's whole frame (ADR 0049's trade).
- **By-eye timings:** the clip is sampled every 33 ms; every value is a tunable above.
- **Cross-plan:** MB7 changes `BattlersView` (frames), MB8 adds `EffectTiming` and the GIF effect import: EF1 and EF7
  reread those files first.
- **Saves:** unchanged. **Performance:** per frame, one dictionary lookup per strip, at most a few dozen digit draws, a
  few extra sprite draws during a blink, a light or a fringe.
