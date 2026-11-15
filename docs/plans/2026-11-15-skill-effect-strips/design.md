# Design: a skill's own effect strip

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Fire explosion height | SE3's import (`GifImportOptions.BattleHeight`) | 443 px, the GIF's own (8 x 443 = 3544 px wide, under 4096) | The strip's square frame side, so its size on screen (decision 655). |
| Fire explosion pace | `content/effect-strips.json`, `FireExplosion.png` `frameMilliseconds` | 110, the GIF's own frame time | How long each frame shows. |
| Fire explosion hit frame | `content/effect-strips.json`, `FireExplosion.png` `hitFrame` | the frame where the burst is widest, by eye on the imported strip (0 to 6) | The frame shown as the numbers land (decision 656). Up to 6 keeps hit frame x 110 ms within the 0.7 s wind-up (`BattlePacing.WindUp`); a longer lead loses its first frames. |
| Background removal | SE3's import (`GifImportOptions.RemovesBackground`) | off: the GIF is transparent around the flame (the animated monsters plan's measurement) | Whether the top-left color is cleared (decision 638). |

No new Game constant: the swing's start (`BattlePacing.SwingStart`) and the unlisted pace
(`EffectStripDefinition.UnlistedFrameTime`) are reused.

## Design overview

### What exists
- **The record** (ADR 0048): `Core/Battle/EffectStripDefinition` gives a strip, by file, its `FrameMilliseconds`,
  `HitFrame` and `SwingFile`; `EffectStripDefinition.Of(file, listed)` returns the record or the unlisted pace. Loaded
  from `content/effect-strips.json` into `ContentCatalog.EffectStrips`; edited in Database > Effect strips....
- **Who names a strip today**: only attackers. `PartyMemberDefinition.AttackEffectFile` and
  `EnemyDefinition.AttackEffectFile`, read through `Game/Battle/BattlerArt.Of(combatant).AttackEffectFile`.
- **The wind-up** (`Game/Battle/ActionWindUp`): `AimedTargets` is an Attack's target or a damaging skill's or item's
  targets; `ImpactFile` is the actor's attack strip when it aims at anyone; `SwingFile` is the impact strip's record's
  swing for an `AttackCommand` only (decision 657); `Sounds` adds the whoosh when there is a swing.
- **The playback** (`Game/Battle/ActionPlayback`, 274 lines): on `ActionStarted` it keeps the actor's attack strip in
  `_attackEffectFile`; each damaging `EffectApplied` adds an `EffectOnTarget` with it, once per target.
- **The views** read files, not attackers: `ImpactEffectsView` draws `windUp.ImpactFile` and `playback.Strips` at each
  file's record pace (`PaceOf(file)`), `SwingView` draws `windUp.SwingFile`. They need no change.
- **The checks**: `Data/Skills/SkillCatalogValidator` and `Data/Items/ItemCatalogValidator` (both through
  `Data/Effects/EffectValidator` for the effect); `Data/Art/ArtReferenceCheck.Battle.cs` checks attack strips by exact
  name and square frames, and every listed record's strip, swing and hit frame.
- **The import** (`Editor/Catalogs/ArtImageImport`, `Editor/GifImport/GifConversion.ConvertToEffect`): a GIF into a
  strip of square frames in `effects/`, its time dropped for the effects' pace; used headlessly in the battle turn
  presentation plan's EF7.
- **Content**: `skills.json` and `items.json` are hand-written (not in the writer's layout; the editor never writes
  them). `fireball` (Omer the mage's, 5 mana, all enemies, fire) exists; `art-src/gifs/FireExplosion.gif` is committed
  and imported nowhere.

### Core changes (SE1)
- `Core/Skills/SkillDefinition.EffectFile` and `Core/Items/ItemDefinition.EffectFile`: `string?`, init, null by
  default. No ctor churn (both are init-only records), no behavior: Core never plays strips.

### Structure and folder dependencies

| New or changed type | Project/folder | Reads | Responsibility |
|---|---|---|---|
| `SkillDefinition.EffectFile` | Core/Skills | - | The strip a skill's action plays instead of its user's attack strip; null for none. |
| `ItemDefinition.EffectFile` | Core/Items | - | The same for an item used in battle. |
| `OwnEffectStripValidator` (new, internal static) | Data/Effects | Core/Effects | Checks the strip a skill or item names for itself: a name when present, on a damaging effect only. |
| `SkillCatalogValidator`, `ItemCatalogValidator` | Data/Skills, Data/Items | Data/Effects (exists) | Call it. |
| `ArtReferenceCheck.Battle.cs` | Data/Art | Core/Content (exists) | Adds the skills' and items' own strips to the attack strips it checks. |
| `ActionStrip` (new, internal static) | Game/Battle | Core/Battle, Core/Skills, Core/Items, `BattlerArt` | Which strip an action plays (its skill's or item's own, else its actor's attack strip) and which swing goes with it: `FileOf(actor, command)`, `SwingFileOf(actor, command, strips)`. |
| `ActionWindUp`, `ActionPlayback` | Game/Battle | `ActionStrip` | Read the action's strip from it instead of `BattlerArt`. |

No new folder edge: Data/Skills and Data/Items already use Data/Effects; Game/Battle already reads every Core folder
named.

### Key decisions
- **The skill names the file; the record keeps the art facts** (decisions 678, 706). A skill's strip plays at its
  record's pace and hit frame, through the same `ImpactEffectsView.PaceOf(file)` lookup the attack strips use.
- **One rule for an action's strip** (`ActionStrip.FileOf` and `ActionStrip.SwingFileOf`):
  - `SkillCommand` whose skill names a strip, or `ItemCommand` whose item names one: that strip; its swing is its
    record's (decision 708).
  - `AttackCommand`: the actor's attack strip; its swing is its record's (decision 657).
  - Any other command: the actor's attack strip, no swing (decision 657's Q2).
  `ActionWindUp` still decides *whether* a strip plays (only when `AimedTargets` isn't empty) and `ActionPlayback`
  still plays it only on damage: `ActionStrip` decides *which*.
- **Damaging actions only** (decision 707, a default): the loader refuses `effectFile` on a skill or item whose effect
  doesn't damage, so `ActionStrip` never hands a strip to an action the screen shows with a cue (decision 101).
- **A backfire** shows its cue and number on the caster, no strip, as today; the enemies still get the action's own
  strip (decision 667's lead included).
- **The swing's place** is unchanged: `SwingView` draws it ahead of the actor whatever the command, from
  `BattlePacing.SwingStart`; a skill's whoosh follows its own opening sound in `ActionWindUp.Sounds` (time order kept).

### Extension points
- **A non-damaging action's strip** (decision 707 lifted): `ActionWindUp.AimedTargetsOf` would aim a skill with its
  own strip whatever its kind, and `ActionPlayback.AddLanding` and the status cue would play it where the effect or the
  status landed. `ActionPlayback` needs splitting first (274 lines).
- **The skills window** (row 7): a strip box over the effects images (the Effect strips window's `BattleArtChoices`
  folder lookup), the `AbsentWhenNullFilter` entry for `(SkillDefinition, "effectFile")`, and the relay of
  `skills.json` into the writer's layout (the catalog windows' first writing slice, as for enemies). Row 9 likewise for
  items.
- **Row 6 (decision 704)**: if a skill's or item's effect becomes a reference to an effect record,
  `OwnEffectStripValidator` runs where the reference is resolved; the field stays on the skill and item, and
  `ActionStrip` is unchanged.
- **A strip on an enemy's skill** needs nothing more: an enemy casting a skill that names a strip plays it, unmirrored
  as every enemy's effect (decision 655).

### Replacement mapping

| Existing code | Replaced by |
|---|---|
| `ActionWindUp`'s `BattlerArt.Of(actor).AttackEffectFile` for `ImpactFile` | `ActionStrip.FileOf(actor, command)` |
| `ActionWindUp`'s `command is AttackCommand && ... .SwingFile` | `ActionStrip.SwingFileOf(actor, command, strips)` |
| `ActionPlayback._attackEffectFile` set from `BattlerArt` on `ActionStarted` | `_stripFile` from `ActionStrip.FileOf(started.Actor, started.Command)` |

`BattlerArt.AttackEffectFile` stays: `ActionStrip` reads it for the default.

## Risks
- **`ActionPlayback.cs` at 274 lines**: SE2 renames one field and replaces one call (net under +5). Anything more there
  (decision 707 lifted) needs its split first.
- **A lead longer than the wind-up**: a late hit frame loses the strip's first frames (decision 656's rule). At 110 ms
  a frame, any hit frame up to 6 fits the 0.7 s wind-up; the developer tunes it in the Effect strips window.
- **The texture limit** (4096 px wide at most, `TextureLimit.MaxSide`, refused by the effects' import): the explosion's
  8 frames at 443 px make 3544 px, so it keeps its own height.
- **A GIF's transparency flag lies** (the animated monsters plan's `GnollArcher.gif`): `FireExplosion.gif` was measured
  transparent around the flame by the animated monsters plan; SE3's hand check would show a backdrop at once if not.
- **First use loads the texture mid-battle** (`TextureLibrary.Get` loads on first use): a large explosion strip may
  hitch on its first cast, as every attack strip's first use can today. Watched in SE3's hand check; a preload is a
  follow-up only if seen.
- **Determinism, saves**: none. A strip is presentation; saves keep skill and item ids only; Core's battle results are
  unchanged by the field.
- **Hand-written content**: a typo in `effectFile` is caught at startup by the art check (decision 709), not in the
  editor, which doesn't show the field until row 7.
