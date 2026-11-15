# A skill's own effect strip (editor push, roadmap row 2)
Status: approved

Approved on 2026-11-15 by delegation (the developer delegated approval of the editor push's sub-plans), with the
devil's advocate's review applied: SE3 uses the GIF's measured values (no decode step); the validators chain the strip
check in `Validate` and leave `FindProblem` untouched; a backfire keeps today's look; row 6's effect references are an
extension point; the bomb names the explosion (the item path's user); `ActionStrip` is a static class.

Planned on 2026-11-15 from roadmap row 2 of [the editor push](../2026-11-15-editor-push/README.md) and its design.md
section 3.6: "a skill gets an optional `effectFile` whose pace, hit frame and swing come from its `effect-strips.json`
record (decision 678)". The developer was away while this was planned: the decisions marked "(default taken while the
developer was away; his to overturn)" are listed under [Defaults to confirm](#defaults-to-confirm).

- Numbers: decisions 706-713. ADR 0051 is **not used**: ADR 0048 gets a dated amendment instead (decision 713).
- Projects: Core (two optional fields), Data (validators, the startup art check), Game (the wind-up and the playback),
  content (one imported strip, its record, its credit, one skill line and one item line). No Editor code, no save change.

## Goal
1. A skill or an item may name its own effect strip (`effectFile`, a file in `content/art/effects/`). When it does, its
   action plays that strip instead of its actor's attack strip: the lead during the wind-up on the targets it aims at,
   the rest where damage landed, at the pace and hit frame of the strip's `effect-strips.json` record, and the swing
   that record names, with its whoosh. Without one, nothing changes: the actor's attack strip, no swing for a skill or
   an item (decision 657 kept).
2. A strip a skill or item names is checked like every other: the loader refuses a blank name or one on an action
   that doesn't damage, and the game's startup art check refuses a missing file or one that isn't whole square frames.
3. Fireball and the bomb play `FireExplosion.png`, imported from `art-src/gifs/FireExplosion.gif` through the editor's
   own GIF import, paced in `effect-strips.json` and credited in `art-sources.json`.

## Order and why
1. **The ADR amendment first** (0048, a dated note): the record now serves a strip a skill or item names, not only an
   attacker's.
2. **SE1, Core and Data**: the field and every check that reads it, so content naming a strip is refused or accepted
   at load and at startup before anything plays it. No content names one yet, so the game's ignoring it is harmless.
3. **SE2, Game**: one rule (`ActionStrip`) says which strip an action plays and whether its swing plays; the wind-up
   and the playback both read it. Its hand check names a strip in a local edit of `skills.json`, reverted after.
4. **SE3, content**: the explosion imported and paced, Fireball and the bomb naming it. Last, since content using the field lands
   with the playback that shows it.

## Guiding rules
- The game's art facts stay in `effect-strips.json` (ADR 0048): the skill names a file, the record says how it plays.
- One place decides an action's strip (`Game/Battle/ActionStrip`); `ActionWindUp` and `ActionPlayback` never work it
  out beside it.
- `ActionPlayback.cs` is 274 lines: SE2 changes it by a renamed field and one call (net under +5 lines), never more.
- Skills and items aren't editor-written yet: the field is hand-written JSON until roadmap row 7 (skills) and row 9
  (items) add a box (decision 710).
- Tests build their own skills and items (`SoundingSkills.WithSound(...) with { EffectFile = ... }`, records built in
  the test); repository tests never pin which skill names which strip.

## Out of scope
- A strip for a heal, a revive or a status-only skill (decision 707): a later plan gives non-damaging actions their
  animation and lifts the refusal.
- Projectiles (decision 665), a strip on the tiered effect records (decision 704; decision 706 says why not).
- The skill and item windows' strip box: rows 7 and 9.
- Removing the unused `AttackEnemy.png` and `AttackPlayer.png` (decision 664, backlog).
- A first-use hitch when a strip loads mid-battle: every strip loads on first use today (design.md, Risks).

## Slices

### Phase 0: the record
- [x] **ADR 0048 amendment** (docs). Write it from the entry under "ADRs needed" below: a dated "Amendment" section in
  `docs/decisions/0048-battle-effect-strips-as-records.md` and its line in `docs/decisions/README.md`.

### Phase 1: a skill's own strip
- [ ] **SE1. A skill or item names its strip, checked at load and at startup** (Core, Data, about 220 lines) →
  [slices/SE1.md](slices/SE1.md)
- [ ] **SE2. The action plays its own strip and its swing** (Game, about 240 lines) → [slices/SE2.md](slices/SE2.md)
- [ ] **SE3. Fireball's and the bomb's explosion** (content, about 25 lines; a throwaway import test, deleted) →
  [slices/SE3.md](slices/SE3.md)

## Decisions
- **706.** The field is `effectFile`, optional, on `SkillDefinition` and `ItemDefinition`: a file name in `effects/`,
  exact case; absent, the action plays its actor's attack strip (decision 678's default). It is not on `Effect`:
  decision 704's tiered effect records (`fire_1` to `fire_5`) will be shared by many skills, which would then share
  one explosion.
- **707.** A skill's or item's own strip plays where an impact strip plays today: on an action that damages (decision
  656). A skill or item naming a strip while its effect doesn't damage (a heal, a revive, a status grant, a rest,
  equipment, a material) is refused at load: "effectFile is only for a damaging effect (decision 707)." A silent
  no-op would look like a bug. (default taken while the developer was away; his to overturn)
- **708.** The swing comes from the strip's record whenever the action plays a strip of its own choosing: an Attack's
  (decision 657), or a skill's or item's own strip, with the swing's whoosh at `BattlePacing.SwingStart` (decision
  658). A skill or item on its actor's attack strip still plays no swing (decision 657's Q2 kept).
- **709.** The startup art check covers skills' and items' own strips as it covers attack strips: present in
  `effects/` by exact name, whole square frames, each named by "skill '<id>'" or "item '<id>'". A strip a record
  lists is already checked for its hit frame (ADR 0048). The editor never refuses missing art (decision 442).
- **710.** Until roadmap row 7 (skills) and row 9 (items), `effectFile` is written by hand: in `skills.json`, the line
  `"effectFile": "FireExplosion.png",` after the skill's `soundFile`; in `items.json`, after the item's `price`. The
  editor neither shows nor writes it. Row 7's first slice that writes `skills.json` adds `(SkillDefinition,
  "effectFile")` to `Data/Content/AbsentWhenNullFilter` (row 9 the item's), so a skill without a strip writes back
  unchanged; no entry lands before a writer exists (no caller).
- **711.** Fireball and the bomb play `FireExplosion.png`: an explosion on every enemy Fireball hits reads as its name
  says, and the bomb gives the item path a user in content. Fire Blast, Meteor and Bomb All keep their actor's attack
  strip; adding the same line to any of them is a one-line change. (default taken while the developer was away; his
  to overturn)
- **712.** `FireExplosion.png` is imported from the GIF as the animated monsters plan measured it (443 x 443, 8
  frames, 110 ms, transparent around the flame): height 443 (8 x 443 = 3544 px, under `TextureLimit.MaxSide`),
  background box off; its record plays at 110 ms a frame, its hit on the frame where the burst is widest (any up to 6
  keeps the lead within the 0.7 s wind-up), no swing. It is listed under `generated` in `art-sources.json`, as the
  roadmap's brief asked (the golden strips' source). (default taken while the developer was away; his to overturn:
  the source, and the pace and hit frame, which he tunes in Database > Effect strips...)
- **713.** ADR 0048 is amended by a dated note rather than a new ADR 0051: decision 678 already chose the option (the
  skill names the strip; the strip's record keeps its pace), and 0048's argument against per-attacker fields (one strip
  shared by thirteen records) doesn't apply to a skill choosing its own art.

## Defaults to confirm
- **707**: own strips on damaging skills and items only, a non-damaging one refused at load. The alternative: heals
  and status skills play theirs too (about one more slice: the status landing has no strip path today, and
  `ActionPlayback` would need a split first).
- **711**: Fireball and the bomb play the explosion (not Fire Blast, Meteor or Bomb All).
- **712**: the import's height (443), pace (110 ms) and hit frame, and the `generated` source in `art-sources.json`.

## Open design questions (developer decides)
None beyond the defaults above.

## ADRs needed
- **ADR 0048, amendment (2026-11-15): a skill's or item's own strip** (decisions 678, 706-709). Write it from this
  entry; the option was chosen in the roadmap (decision 678). Text: a skill or item may name the impact strip its
  action plays (`effectFile`); the strip's record gives its pace, hit frame and swing as for an attacker's strip; an
  action without one plays its actor's attack strip, and a skill or item on it plays no swing (decision 657); only a
  damaging action names one for now (decision 707); the startup check covers the named strips (decision 709). The
  README index line for 0048 gains "amended 2026-11-15: a skill's or item's own strip".
