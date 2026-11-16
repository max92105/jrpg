# Skill and status editors and the animation tools (editor push, roadmap row 7)
Status: approved, in progress

Planned on 2026-11-15 from roadmap row 7 of [the editor push](../2026-11-15-editor-push/README.md) (design.md sections
3.4, 3.5 need 2, 3.8) and the approved row 6 plan, [skills and statuses model](../2026-11-15-skills-and-statuses-model/README.md)
(its decision 815 and design.md section 6, "Notes for row 7"). Approved on 2026-11-15 by the coordinator, the developer
having delegated approval, with the devil's advocate's review applied: the rebinding made catalog-wide, skills and
items alike (B1, decision 975); imported and sheet-cut strips entering the Effect strips session as typed rows, no
record written behind a session (B2, decision 988); the editors built as pages on the mode bar's recipe, their entries
in `Shell/DatabaseMenu.xaml` (B3, decision 976); the slice set reshaped (C1); the balance table moved after row 12
(C2, decision 992); the `effects.json` page called "Effects" (C3); the preview control's pace one-way (C4); several
GIFs keeping their own pace (C5); one tier-id rule (C6); two more tests (C7). Every game-design choice taken while the
developer was away is marked "(default taken while the developer was away; his to overturn)" and listed under
[Defaults to confirm](#defaults-to-confirm).

- Numbers: decisions 971-997. ADR 0062 is **not used** (decision 995).
- Projects: Data (the writer takes three files over; `CatalogReferences`; three `CatalogProblems` entries), Editor
  (three pages, one shared control, the GIF dialog's preview, the strip tool), content (the `skills.json` relay), the
  smoke script. Core only in the last phase (two public readings for the balance table, BT1) and perhaps one
  accessibility word (FX4). No save change, no game rule change.

**Before this plan starts** (each an approved plan of the same push):
- Row 6, every slice (T1-T4, EP, SR1, R1, R2, MS, CH, SW, TT, ST, WK, SL): the pages edit `TieredEffect`,
  `StatusDefinition` with `ActsAt`, `MarkLetter`, a part's `DamageType` and `Stat`, `Effect.Tier`, `Effect.Statuses`
  and `StatusGrant(Status, Turns) { Chance }`, and Data's `CatalogReferenceConverter<T>`.
- The mode bar's stages 1 and 2 ([mode bar](../2026-11-15-mode-bar/README.md), WD1-WD8): `Workspaces/CatalogDialog`
  hosting a page, `Workspaces/IEditedCatalog`, `MainWindow.Database.OpenInDialog`, the Database menu in
  `Shell/DatabaseMenu.xaml`, and the Effect strips window already `Battle/EffectStripCatalogPage` (WD6). Every page
  here follows its [page recipe](../2026-11-15-mode-bar/page-recipe.md).
- Row 5's SW3 (`Sprites/SequenceMarks`, the cells numbered in pick order), which AN6 reuses.
- Row 2 is done (SE1-SE3: `effectFile` on skills and items, Fireball and the bomb name `FireExplosion.png`).
- The balance table (phase 4) waits for row 12 (decision 992).

## Goal
1. **Three Database pages over the battle model** (decision 815): Skills (name, description, icon, sound, its own strip
   with that strip's pace shown, costs as resource rows, the effect with its tier, targeting and hits, the statuses it
   grants with turns and chance, the quick flag), Effects (`effects.json`: `fire_1`...: id, kind, damage type,
   `perPower`, with an "At Power 10: 20 per hit" line) and Statuses (`statuses.json`: name, mark letter, description,
   family, start or end of turn, parts from the closed list with their numbers). `skills.json`, `effects.json` and
   `statuses.json` become editor-written in the writer's layout.
2. **The animation tools he asked for:** one shared preview control (play, pause, step, at its host's typed pace) in
   the Effect strips page, the GIF import dialog (the GIF plays before anything is written, its pace editable) and a
   "Strip from sheet cells" tool that cuts picked cells of any sheet into an effects strip.
3. **After row 12, the balance table ADR 0045 promised:** a read-only page of every class's skills at chosen levels,
   every enemy's skills at its power, the classes' and enemies' HP and Power, and the developer's tier sheet (Power x
   `perPower` x (1 + modifiers)), all through Core's own formulas.

## Order, and why
1. **Skills first (SK1-SK7)**, the page that unblocks the most content: the writer and the relay (SK1), one skill's
   rules and the draft step (SK2), the list's rules (SK3), the page with names and files (SK4, usable from here), then
   one section per slice: costs (SK5), effect (SK6), statuses (SK7). Each slice from SK4 ends with a page that saves
   what it shows.
2. **Effects and statuses (FX1-FX6)**: the two catalogs the skills name. FX1 makes every step that changes them rebind
   whatever names them, catalog-wide (decision 975); the Effects page is one slice (FX2, a page is lighter than a
   window); the Statuses page is its rules (FX4, FX5) then the page (FX6).
3. **Animation tools (AN1a-AN7)**: the playback rules (AN1a), the control in the Effect strips page (AN1b), the GIF
   dialog's preview (AN2), the Effect strips page's own import, where an effect GIF gets its pace (AN4); the strip tool's
   rules (AN5), its window with picking and the preview (AN6), its write into the Effect strips session (AN7).
4. **The balance table after row 12 (BT1-BT3)**: born as a read-only page; its class rows read row 12's skills learned
   by level. Until then the Effects page's "At Power 10" line is the balancing aid (decision 992).

## Guiding rules
- **The page recipe** ([mode bar](../2026-11-15-mode-bar/page-recipe.md)): a page is a `UserControl` over its catalog
  view model with no OK, Cancel or problem line (its host shows them); the catalog view model implements
  `Workspaces/IEditedCatalog` (`Problem`, `ChosenKey`, `Choose`); the editing view model keeps `Start()` and
  `Apply(edited)` (one `CatalogDraft` step) and declares `Workspaces/ICatalogEditing<TEdited>` from the mode bar's WS2
  on; one entry in `Shell/DatabaseMenu.xaml` (its `Tag` the page's title) and one `OpenInDialog` registration in
  `Shell/MainWindow.Database.cs`; typed boxes `UpdateSourceTrigger=PropertyChanged`; bindings to the page's
  DataContext name the page type; per-frame work only through `RenderTick` from `Loaded` to `Unloaded`.
- **ADR 0045's rules hold:** the problem line is the page's own check first, then Data's message through
  `CatalogProblems`; ids typed while new and fixed once in the draft; only records added in the session are removed.
- **Rows over a Core enum are generated from the enum** (ADR 0045): effect kinds, targetings, damage types, part kinds,
  timings, changeable stats; a per-kind label switch throws in its default arm.
- **No editor rule copies a content rule:** a range or combination Data refuses shows as Data's message.
- **The draft holds resolved records** (decision 789): every step changing effects or statuses rebinds the whole
  catalog (decision 975); the Skills page resolves its picks from the records read when it opened.
- **No record behind a session:** an imported or sheet-cut strip becomes a typed row of the Effect strips session; its
  record lands with that page's OK (decision 988).
- **Tests build their own records** (`Editor.Tests/Skills/SkillFixtures`, records built with `with` over the test
  world's catalog); repository tests never pin a skill's numbers.
- **Sizes:** Editor slices about 200 lines, tests at about 10 lines each; a page is lighter than a window (no OK,
  Cancel, problem line or opener of its own). The `skills.json` relay is counted apart.
- **Files near 300 (checked 2026-11-15):** `Shell/MainViewModel.cs` 286 (+3 constructor lines, SK4, FX2, FX6, ending at
  289; the mode bar's WS3 adds 2 later); `Shell/MainViewModel.Database.cs` 88 (+about 30: the three editing
  properties and `ReadSkillChoices`); `Catalogs/CatalogDraft.cs` 272 (untouched: the steps go to the new
  `CatalogDraft.Skills` partial, SK2); `Battle/EnemyCatalogWindow.xaml.cs` 205, or its page after WD5 (AN2 moves its GIF
  question out, about -35); `Battle/EffectStripViewModel.cs` 153 (AN1b takes its preview out); test files past 200:
  `EffectStripViewModelTests` 259 and `EffectStripCatalogViewModelTests` 210 (about 240 after WD6: AN4 starts the
  sibling `EffectStripImportTests`).
- **Every switch over kinds a slice extends** names its helper: `SkillEffectSection.FieldsFor(kind)` (SK6),
  `StatusPartRow.AmountLabel` (FX4); each default arm throws.
- **Hand checks:** these pages show no Mana Seed art; the strip tool's hand check uses the repository's
  `FireExplosion.png` as its sheet, and the GIF checks `art-src/gifs/`. Every slice adding a page or XAML runs
  `.claude/scripts/open-editor-windows.ps1` (no `ERROR` line, no binding-error line, EW0).

## Out of scope
- Adding a skill to a class (`classes.json` `skillIds`, row 12) or to an enemy's tactics (row 8): the Skills page
  shows who uses a skill, read-only (decision 978).
- An Edit... from the Skills page's strip box: a page can't open a nested window before the mode bar's WS6, which
  shows the Effect strips page on the record (its decision 928); until then a read-only pace line (decision 982).
- Items' forms, uniques and affixes (row 9); items are rebound already, and IU5 adds only its item check to
  `OfTieredEffects` and `OfStatuses` (decision 975).
- Per-frame times in a strip or loop (decision 632, backlog), scaling cells in the strip tool, a projectile.
- The older previews (Enemies Art tab, the Animate panel, the Character sprites walk) switching to the shared control:
  when next touched (roadmap design 3.4). The Character sprites' per-facing counters ("2/4") and the playing cell
  outlined stay on their backlog line (decision 991).
- Dominance, crit, gear, resistance and hit chance in the balance table (decision 993).
- Making `Effect.Power` optional in `skills.json` (decision 972).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: the Skills page
- [x] **SK1. The writer takes over `skills.json`, `effects.json` and `statuses.json`** (Data + content, two commits:
  the relay alone, then about 190 lines) → [slices/SK1.md](slices/SK1.md)
- [x] **SK2. A skill's texts and files as typed, and the draft's skills step** (Data + Editor, about 200 lines) →
  [slices/SK2.md](slices/SK2.md)
- [x] **SK3. The skill list: Add, Duplicate, Remove, who uses a skill, the strip's pace, `IEditedCatalog`** (Editor,
  about 230 lines) → [slices/SK3.md](slices/SK3.md)
- [x] **SK4. Database > Skills...: the page** (Editor + script, about 170 lines) → [slices/SK4.md](slices/SK4.md)
- [x] **SK5. A skill's costs as resource rows** (Editor, about 200 lines) → [slices/SK5.md](slices/SK5.md)
- [x] **SK6. A skill's effect: tier, kind, targeting, hits** (Editor, about 240 lines) → [slices/SK6.md](slices/SK6.md)
- [x] **SK7. The statuses a skill grants, with turns and chance** (Editor, about 180 lines) →
  [slices/SK7.md](slices/SK7.md)

### Phase 2: effects and statuses
- [x] **FX1. Everything naming an edited effect or status follows it: `CatalogReferences`, Data's checks and the draft
  step** (Data + Editor, about 220 lines) → [slices/FX1.md](slices/FX1.md)
- [x] **FX2. Database > Effects...: the tier ids, the rules and the page** (Editor + script, about 290 lines) →
  [slices/FX2.md](slices/FX2.md)
- [x] **FX4. A status as typed: its fields and parts** (Editor, perhaps one Core accessibility word, about 220
  lines) → [slices/FX4.md](slices/FX4.md)
- [x] **FX5. The statuses' list, New tier, Data's check and the draft step** (Data + Editor, about 210 lines) →
  [slices/FX5.md](slices/FX5.md)
- [x] **FX6. Database > Statuses...: the page** (Editor + script, about 170 lines) → [slices/FX6.md](slices/FX6.md)

### Phase 3: the animation tools
- [x] **AN1a. The preview's playback rules** (Editor, about 140 lines) → [slices/AN1a.md](slices/AN1a.md)
- [x] **AN1b. The shared preview control, first in the Effect strips page** (Editor, about 140 lines) →
  [slices/AN1b.md](slices/AN1b.md)
- [x] **AN2. The GIF import plays the GIF before it writes, its pace editable for a battle sprite** (Editor, about 220
  lines) → [slices/AN2.md](slices/AN2.md)
- [x] **AN4. The Effect strips page imports PNGs and GIFs, an effect GIF's pace becoming its row's** (Editor, about
  200 lines) → [slices/AN4.md](slices/AN4.md)
- [x] **AN5. Strip from sheet cells: the cut and the picks as typed** (Editor, about 210 lines) →
  [slices/AN5.md](slices/AN5.md)
- [x] **AN6. Strip from sheet cells: the window, picking and the preview, opened from the Effect strips page** (Editor
  + script, about 220 lines) → [slices/AN6.md](slices/AN6.md)
- [x] **AN7. Strip from sheet cells writes the strip into the Effect strips session** (Editor, about 200 lines) →
  [slices/AN7.md](slices/AN7.md)

### Phase 4: the balance table (after row 12)
- [ ] **BT1. Core's readings and the table's class skill rows** (Core + Editor, about 240 lines) →
  [slices/BT1.md](slices/BT1.md)
- [ ] **BT2. Enemies' skills, classes and enemies, and the tier sheet rows** (Editor, about 180 lines) →
  [slices/BT2.md](slices/BT2.md)
- [ ] **BT3. Database > Balance table...: the read-only page** (Editor + script, about 170 lines) →
  [slices/BT3.md](slices/BT3.md)

Files `slices/SK8.md`, `FX3.md`, `AN1.md`, `AN3.md` and `AN8.md` are superseded stubs to delete.

## Decisions

Numbers 971-997. "(default ...)" marks a game-design choice taken while the developer was away; his to overturn.

Plan:
- **971.** Four phases: Skills (SK1-SK7), effects and statuses (FX1, FX2, FX4-FX6), the animation tools (AN1a, AN1b,
  AN2, AN4-AN7), then the balance table after row 12 (BT1-BT3). The plan starts once row 6, the mode bar's stages 1
  and 2 and row 5's SW3 are done.

Files and the draft:
- **972.** The writer takes over `skills.json`, `effects.json` and `statuses.json` (ADR 0045's relay: `skills.json`
  once, in a commit of its own; the other two are in its layout since row 6's T2 and SR1). `skills.json` is written
  with the two `CatalogReferenceConverter`s registered on that file's write only, so a tier and a grant's status are
  written as ids (decision 789). `"power": 0` stays beside a tier (decision 792): making `Effect.Power` optional means
  reordering `Effect`'s positional parameters (33 source and 111 test sites). A grant's `chance` is always written, 100
  included: no "absent when 100" filter for one field.
- **973.** Default-valued fields are left out of `skills.json`: a skill's `effectFile` while null (decision 710),
  `resourceChanges` and `requirements` while empty, `isQuick` while false; an effect's `range` while 0, `hits` while
  1, `damageType` and `tier` while null, `statuses` while empty.
- **974.** Skills are checked with what names them. The writer's `CatalogProblems.CheckSkills` runs the skills' and
  the classes' checks (a class's skills use only its resources, a requirement within its max); the enemies' check
  already runs in `CheckCatalogs`. `CatalogProblems.OfSkills` (the page's problem line) runs `CheckSkills` and the
  enemies' check with the edited skills. Row 12's CL1 extracts `CheckClasses`; `CheckSkills` then calls it.
- **975.** The rebinding is catalog-wide: `Data/Content/CatalogReferences.Rebound(ContentCatalog catalog)` gives the
  catalog with every skill's and item's `Effect` pointed at the tiered effect and statuses of the same ids. The steps
  are `catalog => CatalogReferences.Rebound(catalog with { TieredEffects = after })` and the same with `before` (and
  likewise for `Statuses`); `OfTieredEffects` and `OfStatuses` check what names the records (the skills, through
  `CheckSkills`) over the rebound catalog. Row 9's IU5 only adds its item check there; the rebinding already covers its
  items.
- **976.** The pages: Database > Skills..., Effects... and Statuses..., each an entry in `Shell/DatabaseMenu.xaml`
  beside Effect strips (the page titles "Skills", "Effects", "Statuses"). "Effects" is the `effects.json` page, its
  tooltip "effects.json: fire_1, heal_2...; an image's pace is Effect strips"; its code keeps the `TieredEffect...`
  names. The strip tool has no session, so no menu entry: a button on the Effect strips page opens it.

The Skills page:
- **977.** Skills in id order, added ones after; Add starts a skill with an empty id, name and sound and the first
  damage tier by id on a single enemy, so the next things typed make it valid; Duplicate copies the chosen one after
  it, its id to type (decision 585's pattern); Remove only an added one (decision 581).
- **978.** (default taken while the developer was away; his to overturn) A new skill joins no class: the page shows,
  read-only, "Learned by: Mage" (classes listing it) and "Used by: Goblin Chief" (enemies whose tactics or phases name
  it); a class gains it in `classes.json` until row 12, an enemy in `enemies.json` until row 8.
- **979.** Costs are two row lists, as Core holds them (design 3.8): Changes (a resource and an amount, negative spends,
  positive gains) and Needs (a resource and its minimum); the resource box offers every class's resources labelled
  with their classes ("Mana (mage, warlock)"); a summary line reads "Spends 2 mana; needs 1 faith".
- **980.** The effect: a Tier box (none, or a record: `Effects/TierChoice`, which row 9's IU12 reuses), a Kind box
  (every `EffectKind` but `rest`, which Data refuses on a skill), Damage type, Power, Targeting (every `Targeting`),
  Range (enabled for `alliesInRange` only) and Hits. Choosing a tier sets the kind and damage type from the record and
  greys them and the power box (written 0, decision 792); with no tier, power and type are typed.
- **981.** Grants are rows in order (decision 809): a status (the draft's statuses, "poison_1: Poison"), turns (1 for a
  new row) and chance in percent (100 for a new row); added after the chosen row, moved, removed.
- **982.** The strip box offers the effects images and "(the actor's attack strip)" for none; under it a read-only
  line gives the chosen strip's pace from the draft ("80 ms, hit on frame 3 (Database > Effect strips)"; "80 ms, hit on
  frame 1, unlisted (Database > Effect strips)" for a strip with no record). No Edit... button until the mode bar's WS6.

Effects and statuses:
- **983.** The Effects page: id, kind (`damage` or `restoreHp`), damage type (damage only), `perPower`; a "Used by"
  line (skills naming it) and an "At Power 10: 20 per hit" line through `DamageFormula.TieredAmount` (the Power shown is
  a tunable). Add starts with an empty id; Duplicate copies the chosen one with the next tier's id filled in (`fire_2`
  after `fire_1`, through `Catalogs/TierIds`), still editable; Remove only an added one.
- **984.** The Statuses page: id, name, description, harmful, family (an editable box offering the draft's families;
  empty means its own id), acts at (start or end of turn), mark letter (empty for the name's first), and parts as rows
  (a kind from `StatusPartKind`; an amount whose label the kind gives, hidden for `skipTurn` and `drawAttacks`; a
  damage type for `damageEachTurn` only; a stat for `statChange` only, from `StatusModifiers.ChangeableStats`); a "Used
  by" line names the skills granting it.
- **985.** (default taken while the developer was away; his to overturn) New tier copies the chosen status as the next
  tier of its family: its id from `Catalogs/TierIds.Next` (the id's stem, without a trailing `_<number>`, and the
  highest number of that stem plus one: `poison_2` from `poison_1`, `stun_1` from `stun`), its family the chosen one's
  family key, its parts and texts copied, so the family rule holds until an amount is retyped. The Effects page's
  Duplicate uses the same rule.

The animation tools:
- **986.** One shared control, `Animation/FramePreview`: the frames, Play or Pause, Step back and Step forward (which
  pause), and "frame 3 of 8" (", the hit" on a marked frame). It takes `int? FrameMilliseconds` and `int? HitFrame`
  one-way; each host keeps its own typed box (the Effect strips page's, the GIF dialog's, the strip tool's), so the
  preview and what is written can't differ. Its rules are `Animation/FramePlayback`, headless; its per-frame work goes
  through `Hosting/RenderTick` from `Loaded` to `Unloaded`.
- **987.** The GIF dialog shows the first chosen GIF playing, composed as the import composes it and cleared when the
  background box is ticked; its pace box is filled with the GIF's most common delay (decision 632) and a battle
  sprite's loop takes the typed pace. With several GIFs chosen, the pace box greys ("each keeps its own pace") and the
  first one plays.
- **988.** (default taken while the developer was away; his to overturn) An effect GIF imported on the Effect strips
  page enters that page's session as a typed row at the dialog's pace (a replaced strip's row keeps its hit frame below
  its new frames, else 0, and its swing); the record lands with the page's OK, as any typed row. An effect GIF imported
  from the Enemies page's Art tab keeps decision 639 (its pace dropped): there the dialog greys the pace box with the
  note "set its pace in Effect strips".
- **989.** The Effect strips page gets Import... (PNGs and GIFs into the effects folder) through the shared import, so
  a skill's strip is imported where it is paced.
- **990.** (default taken while the developer was away; his to overturn) Strip from sheet cells, opened from the
  Effect strips page: Open sheet... reads a PNG from anywhere; the cut is columns and rows (`GridCut`); with Pick on, a
  click adds the cell under it, numbered in pick order (a cell may repeat), and a click on a row's number adds every
  cell of that row in order; the preview plays the picked cells at the typed pace; OK writes the strip into `effects/`
  through Data's import check (a file copy, never undone), each cell at its own size padded to a square (as an effect
  GIF's frames), asking before replacing an image, and hands it to the Effect strips session as a typed row at its
  pace. No scaling.
- **991.** The Character sprites window's per-facing counters stay on their backlog line (cut at review): the shared
  control reaches that window when it is next touched.

The balance table:
- **992.** (default taken while the developer was away; his to overturn) The balance table moves after row 12: it is
  born as a read-only page (the page recipe's step 8) and its class rows read row 12's skills learned by level. Until
  then the Effects page's "At Power 10" line is the balancing aid. Its boxes: three levels (1, 10 and 20), a damage
  modifier in percent (0: the sheet's "other" modifiers, applied as Core applies the modifiers' sum) and three Powers
  for the tier sheet (1, 10 and 50). Tabs: Skills (each class's skills at the three levels, each enemy's at its power,
  per hit and times its hits), Classes and enemies (HP and Power at the levels; an enemy's HP, power and attack), Tiers.
- **993.** The table's numbers are Core's: a class member at a level wearing nothing (`Battle/ClassBattleStats.At`), an
  effect's amount per hit as the battle scales it (`Battle/EffectAmounts.PerHit`, over `EffectScaling`), the tier
  sheet through the same reading. No dominance, crit, resistance, gear or hit chance: each needs a target, a roll or an
  outfit the table doesn't have.
- **994.** The table is list rows with shared column widths (`Grid.IsSharedSizeScope`) in the dark theme, not a
  `DataGrid`, which has no dark template.

Plan-wide:
- **995.** No new ADR: ADR 0045 covers the pages and the table (with the mode bar's ADR 0060 for the page host), ADR
  0056 the rebinding (decision 789), ADR 0048 the strip records; decision 988 keeps decision 639 where it applied.
  ADR 0062 stays free.
- **996.** `.claude/scripts/open-editor-windows.ps1`'s default list gains `'Database>Skills...'`,
  `'Database>Effects...'`, `'Database>Statuses...'` and, after row 12, `'Database>Balance table...'`; AN6 teaches it a
  nested path, `'Database>Effect strips...>Strip from sheet cells...'` (a button invoked inside the opened page), so
  the strip tool is opened by script.
- **997.** No save format change; no game rule changes. Core gains two public readings for the table (BT1), each a
  wrapper over the battle's own rule with a test.
- **1001.** (taken at FX2: `DamageFormula` is internal to Core) The Effects page's "At Power 10" line reads a third
  public reading, brought forward from the balance table's: `TieredEffect.AmountAt(int power)`, one hit before
  modifiers and target through `DamageFormula.TieredAmount`; BT2's tier sheet adds the modifiers when it needs them.

## Defaults to confirm
Taken while the developer was away, each his to overturn:
1. **992.** The balance table waits for row 12 (its class rows read skills learned by level); until then the Effects
   page's "At Power 10" line. Its starting levels 1, 10, 20, modifier 0 %, Powers 1, 10, 50; three tabs.
2. **978.** A new skill joins no class or enemy from this page; the page shows who uses it.
3. **988.** An effect GIF imported on the Effect strips page keeps its typed pace as a typed row; from the Enemies page
   its pace is dropped as before.
4. **990.** The strip tool keeps cells at their size, padded to squares; no scaling box; a row's number picks the row.
5. **985.** New tier names `poison_2` after `poison_1` and keeps the family's parts; the Effects page's Duplicate fills
   `fire_2` the same way.
6. **991.** The Character sprites counters stay in the backlog.

## Open design questions (developer decides)
- None blocking; the defaults above are the questions. For later: a Learned by box editing `classes.json` (row 12),
  hits-to-fell against a chosen enemy in the balance table.

## Notes for other rows
- **Mode bar (row 11):** its WS2 declares `ICatalogEditing<T>` on this plan's three editing view models with the
  others; its WS6 may add the Skills page's strip Edit... (its decision 928), showing the Effect strips page on the
  skill's strip.
- **Row 9 (items):** `CatalogReferences.Rebound` already rebinds items; IU5 adds its item check to `OfTieredEffects`
  and `OfStatuses`; IU12 reuses `Effects/TierChoice`.
- **Row 12 (classes):** CL1 extracts `CheckClasses` and SK1's `CheckSkills` calls it; the balance table (phase 4)
  follows it.

## ADRs needed
- None (decision 995).
