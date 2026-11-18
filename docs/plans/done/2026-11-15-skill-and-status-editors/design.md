# Skill and status editors: design

Read with the [README](README.md). Sections: 1 tunables, 2 design overview, 3 risks.

## 1. Tunables and placeholders

None of these is game content: they are where an editor box starts, named constants in the type that uses them (an
editor's UI starting values have no content home, as the cinematics plan found).

| Name | Where | Value | Meaning |
|---|---|---|---|
| `ShownPower` | `Effects/TieredEffectViewModel` | 10 | The Power its "At Power 10: 20 per hit" line uses (decision 983). |
| New grant row | `Skills/StatusGrantRow.New` | turns 1, chance 100 | A grant row as Add makes it (decision 981). |
| New skill's effect | `Skills/SkillViewModel.New` | the first damage tier by id, `singleEnemy` | So Add leaves only the id, name and sound to type (decision 977). |
| Strip tool's pace | `Animation/StripFromCellsViewModel` | 80 ms (`EffectStripDefinition.UnlistedFrameTime`) | A strip at this pace needs no record (ADR 0048). |
| Preview pixels | `Animation/FramePreview` | nearest neighbor, `Stretch="Uniform"` in the host's area | Pixel art stays crisp. |
| `StartingLevels` | `Balance/BalanceTableViewModel` (phase 4) | 1, 10, 20 | The three level boxes the table opens with (decision 992). |
| `StartingPowers` | `Balance/BalanceTableViewModel` (phase 4) | 1, 10, 50 | The tier sheet's three Power boxes (decision 992). |
| Damage modifier | `Balance/BalanceTableViewModel` (phase 4) | 0 % | The sheet's "other" modifiers, added to the modifiers' sum (decisions 783, 786). |

## 2. Design overview

### 2.1 What exists (2026-11-15, after rows 2, 5, 6 and the mode bar's stages 1 and 2)
- **Content:** `skills.json` hand-written (class order, inline objects); `effects.json` and `statuses.json` in the
  writer's layout from row 6 (T2, SR1, tested by `ContentWriterRepositoryTests`). A skill's `Effect` holds its
  `TieredEffect` (`Tier`) and each grant its `StatusDefinition`, resolved at load by Data's
  `CatalogReferenceConverter<T>` (decision 789), whose `Write` writes the id back (tested, no production caller yet).
  Items are read with the same converters (row 6's SW).
- **Data:** `ContentWriter.Catalogs` checks and serializes each editor-written catalog (one place to add one; its
  `CheckCatalogs` runs `CheckEnemies` already); `CatalogProblems.OfX` gives the loader's message for an edited list;
  the `AbsentWhen*Filter`s leave default fields out. The loader checks classes (`ClassCatalogValidator`: a class's
  skills use its own resources, ADR 0020) and enemies (`EnemyTacticsValidator`: no class resource; row 6's T3: a tiered
  skill needs Power) against the skills.
- **Editor:** every Database editor is a page in `Workspaces/CatalogDialog`, opened by `MainWindow.Database
  .OpenInDialog` from `Shell/DatabaseMenu.xaml` (the mode bar's WD1-WD8, its [page recipe](../2026-11-15-mode-bar/page-recipe.md));
  the Effect strips page (`Battle/EffectStripCatalogPage`, WD6) plays the chosen strip on its own `RenderTick`
  (`EffectStripViewModel.PreviewLoop`, `FrameLineAt`); GIFs are imported only from the Enemies page's Art tab
  (`AskGifOptions` opens `GifImportWindow`: height and background, no preview), converted by `GifImport/GifConversion`
  (an effect's GIF time dropped, decision 639); row 5's SW3 picks cells on a sheet by click and numbers them in pick
  order (`Sprites/SequenceMarks`).
- **Core:** `Battle/EffectScaling.ScaledFor(effect, user)` and `Battle/MemberBattleStats.Of(member)` are internal;
  `DamageFormula.TieredAmount` (T1), `ClassGrowth.StatsAt`, `MemberSheet.Of` and `Combatant.FromEnemy` are public.

### 2.2 Core changes (phase 4, BT1)
| Type | Change |
|---|---|
| `Battle/ClassBattleStats` (new, public static) | `At(ClassDefinition characterClass, int level, StatRules rules)`: a member of the class at the level, nothing worn, as `MemberBattleStats` builds a member's stats. |
| `Battle/MemberBattleStats` | `Of(PartyMember)` maps through a new internal `Of(StatSheet sheet, DamageType attackType)`, which `ClassBattleStats` shares. |
| `Battle/EffectAmounts` (new, public static) | `PerHit(Effect effect, CombatStats user)`: the scaled effect's power (`EffectScaling.ScaledFor`), the HP one hit deals or heals before its target. |

### 2.3 Data changes
| Slice | Change |
|---|---|
| SK1 | `ContentFileWriter.Serialize(content, params JsonConverter[] converters)`; `ContentWriter.Catalogs` checks and writes `effects.json`, `statuses.json`, `skills.json` (the last with both reference converters); `CatalogProblems.CheckSkills` (skills and classes), `CheckTieredEffects`, `CheckStatuses` (internal); filters of decision 973; `ContentCatalogLoader.SkillsFile` and `ClassesFile` internal. |
| SK2 | `CatalogProblems.OfSkills(skills, content)`: `CheckSkills`, then the enemies' check with the edited skills. |
| FX1 | `Content/CatalogReferences` (new, public static): `Rebound(ContentCatalog)`, every skill's and item's effect pointed at the records of the same ids; `CatalogProblems.OfTieredEffects(effects, content)`: the records' rules, then `CheckSkills` over the rebound catalog. |
| FX5 | `CatalogProblems.OfStatuses(statuses, content)`: the records' rules, then `CheckSkills` over the rebound catalog. |

### 2.4 Editor structure: new types
Folders follow Core's: `Skills/`, `Effects/` (the tiered effects), `Statuses/`; `Animation/` for the shared control and
the strip tool; `Balance/` for the table (phase 4).

| Type (slice) | Responsibility |
|---|---|
| `Catalogs/CatalogDraft.Skills` partial (SK2, FX1, FX5) | `SetSkills`, `SetTieredEffects`, `SetStatuses`, each one undo step compared by content. |
| `Catalogs/TierIds` (FX2) | The id of a record's next tier: its stem and the stem's next number. |
| `Skills/SkillViewModel` (SK2) | One skill as typed: id, name, description, icon, sound, strip, quick flag; holds its sections. |
| `Skills/SkillUsers` (SK3) | Who uses a skill: the classes listing it and the enemies whose tactics or phases name it. |
| `Skills/SkillChoices` (SK3) | What the Skills page's boxes offer, read when it opens: icons, sounds, effects images and their paces, resources, tiers, statuses. |
| `Skills/SkillCatalogViewModel` (SK3) | Every skill on the page, Add, Duplicate, Remove while new, the problem line; `IEditedCatalog`. |
| `Skills/SkillEditingViewModel` (SK3) | Starts the page from the draft and applies its session as one step. |
| `Skills/SkillCatalogPage` (SK4) | The page: skills left, the chosen one's sections right. |
| `Skills/ResourceChoice` (SK5) | One resource the cost boxes offer: its id and a label naming its classes. |
| `Skills/ResourceChangeRow` (SK5) | One resource change as typed. |
| `Skills/ResourceNeedRow` (SK5) | One requirement as typed. |
| `Skills/SkillCosts` (SK5) | A skill's change rows and need rows, their order and their summary line. |
| `Effects/TierChoice` (SK6) | One entry of a Tier box: a record with its line, or none (row 9's IU12 reuses it). |
| `Skills/SkillEffectSection` (SK6) | A skill's effect as typed: tier, kind, damage type, power, targeting, range, hits. |
| `Skills/StatusChoice` (SK7) | One status the grant box offers: its id and line. |
| `Skills/StatusGrantRow` (SK7) | One grant as typed: status, turns, chance. |
| `Skills/SkillGrants` (SK7) | A skill's grant rows in order. |
| `Effects/TieredEffectViewModel` (FX2) | One tiered effect as typed, with its used-by and amount lines. |
| `Effects/TieredEffectCatalogViewModel` (FX2) | Every tiered effect on the page, Add, Duplicate, Remove while new, the problem line; `IEditedCatalog`. |
| `Effects/TieredEffectEditingViewModel` (FX2) | Starts the page and applies its session as one step. |
| `Effects/TieredEffectCatalogPage` (FX2) | The Effects page. |
| `Statuses/StatusPartRow` (FX4) | One part as typed: kind, amount, damage type, stat. |
| `Statuses/StatusViewModel` (FX4) | One status as typed: texts, family, timing, mark letter, part rows. |
| `Statuses/StatusCatalogViewModel` (FX5) | Every status on the page, Add, Duplicate, New tier, Remove while new, the problem line; `IEditedCatalog`. |
| `Statuses/StatusEditingViewModel` (FX5) | Starts the page and applies its session as one step. |
| `Statuses/StatusCatalogPage` (FX6) | The Statuses page. |
| `Animation/FramePlayback` (AN1a) | Which frame a preview shows: playing at a frame time, paused, stepped. |
| `Animation/FramePreview` (AN1b) | The shared control: the frame, Play or Pause, the steps, the frame line. |
| `Animation/PreviewFrames` (AN1b, AN2, AN6) | Makes the images a preview plays: a strip's frames, pixel frames, a sheet's picked cells. |
| `GifImport/ComposedGif` (AN2) | A GIF's whole frames at its own size and its most common frame time, read once for the preview. |
| `Animation/CellStrip` (AN5) | The strip the tool writes: its file name, cell size, picked cells and pace. |
| `Animation/StripFromCellsViewModel` (AN5) | The strip tool's state: the sheet's size, the cut, the picks, the name and pace, the problem line. |
| `Animation/StripFromCellsWindow` (AN6) | The strip tool's window (a plain modal window: no catalog session). |
| `Animation/SheetCells` (AN7) | Cuts the picked cells out of a sheet's pixels into frames. |
| `Catalogs/CellStripImport` (AN7) | Writes cut frames as an effects strip through Data's import check. |
| `Balance/...` (BT1-BT3) | The table's rows, view model and read-only page (phase 4, after row 12). |

### 2.5 Folder dependencies (Editor)
| From | To | Why |
|---|---|---|
| `Skills`, `Effects`, `Statuses` | `Catalogs` (`TypedNumber`, `TierIds`), `Events` (`ListOrder`), `Objects` (`ObjectIds`), `Workspaces` (`IEditedCatalog`), Data `Content`, Core | rows, ids, problems, the page contract |
| `Skills` | `Effects` (`TierChoice`) | the Tier box |
| `Animation` | `Hosting` (`RenderTick`), `GifImport` (`FrameStrip`), `Tiles` (`GridCut`), `Sprites` (`SequenceMarks`, `SpriteSheetMarks`), `Catalogs` (`TypedNumber`), `Viewport` (`ZoomedScroller`) | the control's tick, the cut, the marks |
| `Battle` | `Animation` (`FramePreview`), `GifImport` (`ComposedGif`), `Catalogs` | the Effect strips page and the GIF dialog host the control |
| `Catalogs` | Core, Data, `GifImport` (`FrameStrip`) | the draft's steps, `TierIds`, `CellStripImport` (as `ArtImageImport` already reads `GifImport`) |
| `Shell` | every folder above | registers the pages, opens the strip tool |

No cycle: `Effects` never reads `Skills`; `Animation` never reads `Battle` (the Effect strips page reaches the strip tool
through a callback the shell gives it, as the Enemies page reaches the Drop tables page); `Catalogs` reads no feature
folder of this plan. Data's `Content` gains `CatalogReferences` over Core's `Skills`, `Items`, `Effects` and `Statuses`
(existing edges).

### 2.6 Key decisions (README numbers)
- **Writing references (972, 789):** `ContentWriter` serializes `skills.json` with
  `new CatalogReferenceConverter<TieredEffect>(catalog.TieredEffects, effect => effect.Id, "effect")` and the status
  one, through `ContentFileWriter.Serialize(content, converters)`; never on `effects.json` or `statuses.json`, whose own
  records must be written whole.
- **Rebinding, catalog-wide (975):** `CatalogReferences.Rebound(catalog)` returns the catalog with `Skills` and `Items`
  rebuilt: each record whose effect's tier or grants differ (by reference) from the catalog's records of those ids
  gets the effect `with` them; a record already right is kept as the same instance. `SetTieredEffects` and
  `SetStatuses` record one `CatalogEdit`: do `catalog => CatalogReferences.Rebound(catalog with { TieredEffects =
  after })`, undo the same with `before`. A record names an id the step removes only if a later step added it, and
  steps undo in order, so `Rebound` throws on a missing id (a bug, never input).
- **Picks resolve when the page applies:** the Skills page keeps a tier and a status as ids; `SkillViewModel.ToSkill`
  resolves them from the records `SkillChoices` read when the page opened (no other session can change them while the
  page is open: one session at a time).
- **Checks across files (974):** an edit to skills can break `classes.json` and `enemies.json`, which the editor doesn't
  write; `OfSkills` runs their checks with the edited skills; the writer refuses the save the same way.
- **The shared preview (986):** `FramePlayback` advances by the render clock's delta (`RenderingEventArgs
  .RenderingTime`), never the wall clock, and uses Core's `TileAnimation.FrameAt`; a paused or stepped preview holds its
  frame; a new frame time keeps the shown frame. `FramePreview` reads `FrameMilliseconds` and `HitFrame` one-way: its
  host's typed box is the only box.
- **Strips enter the Effect strips session (988, 990):** an import and the strip tool both copy a file through Data's
  check (never undone, as `ArtImageImport`) and hand the page a `SheetImportResult` whose `Animations` carry the typed
  pace; `EffectStripCatalogViewModel.TakeImport` makes each a typed row; the record lands with the page's OK.

### 2.7 Page sketches
- **Skills:** skill list left (id, name; Add, Duplicate, Remove); right, scrolling: Id, Name, Description (two lines),
  Icon and Sound, Strip with its pace line, Quick; "Learned by" and "Used by"; Costs (Changes rows, Needs rows,
  summary); Effect (Tier, Kind, Damage type, Power, Targeting, Range, Hits); Statuses (grant rows with Add, Remove, Up,
  Down).
- **Effects:** list left (Add, Duplicate, Remove); Id, Kind, Damage type, Per Power, the Used by and At Power lines.
- **Statuses:** list left (Add, Duplicate, New tier, Remove); right: Id, Name, Description, Harmful, Family, Acts at,
  Mark letter, Used by; part rows (Kind, amount with its label, Damage type, Stat) with Add, Remove, Up, Down.
- **Strip from sheet cells** (a window over the Effect strips page): Open sheet... and the file line; the sheet zoomable
  with its grid, row and column numbers and the pick numbers; Columns, Rows, the cut line; Pick, Remove last, Clear,
  the picks line; the preview; Frame time; Strip name; OK, Cancel.

### 2.8 Extension points
- **Row 9:** `CatalogReferences.Rebound` already rebinds items; IU5 adds its item check to `OfTieredEffects` and
  `OfStatuses`; IU12 reuses `Effects/TierChoice` and the Statuses rows.
- **Mode bar's WS6:** a strip Edit... on the Skills page, showing the Effect strips page on the skill's strip.
- **Row 12:** a Learned by box editing `classes.json`; CL1's `CheckClasses` called by `CheckSkills`; the balance table.
- **Per-frame times** (decision 632): `FramePlayback` would take a list of times; the control is the one place.
- **The older previews** (Enemies Art tab, Animate panel, Character sprites walk with its counters) switch to
  `FramePreview` when next touched.

### 2.9 Replacement mapping
| Today | Becomes | Slice |
|---|---|---|
| `skills.json`, `effects.json`, `statuses.json` hand-written | written by `ContentWriter` | SK1 |
| `EffectStripViewModel.PreviewLoop`, `FrameLineAt`, the page's `ShowFrame` and its tick | `Animation/FramePreview` over `FramePlayback` | AN1b |
| `AskGifOptions`, `HeightOf` in the Enemies page | `GifImportWindow.AskFor(owner, paths, pacing)` over `ComposedGif` | AN2 |
| `GifImportOptions(BattleHeight, RemovesBackground)` | the same with `FrameMilliseconds`, the typed pace | AN2 |
| An effect GIF's time dropped (decision 639) | kept as its row's pace when imported on the Effect strips page | AN4 |

## 3. Risks
- **The relay (SK1):** the first save rewrites `skills.json` whole (sorted by id, one field per line); its commit holds
  only the relay, checked by the load-write-load test. A hand edit to `skills.json` while the editor is open is refused
  at the next save that changes skills (decision 559).
- **Stale records (975):** a skill or item holding an old record would compare as changed and show stale numbers in
  the balance table; every step changing effects or statuses rebinds the whole catalog, and
  `CatalogDraftTieredEffectTests` and `CatalogDraftStatusTests` assert the skills hold the new records after the step,
  the old ones after undo, and the new ones again after an undo and redo of a later skills step.
- **Cross-file refusals (974):** adding a mana cost to a skill a warrior class lists, or to a skill an enemy uses, is
  refused with `classes.json`'s or `enemies.json`'s message; the problem line says which, OK greys.
- **The mode bar's order:** this plan needs its stages 1 and 2; if WD6 hasn't converted the Effect strips window, AN1b,
  AN4 and AN6 work on `EffectStripCatalogWindow` the same way and WD6 carries the control over.
- **XAML theming:** combo boxes and scroll bars have dark templates; a `ToggleButton` (Pick, Play) is `BasedOn` the
  theme's button style; the table avoids `DataGrid` (decision 994).
- **The render clock:** `FramePreview` ticks through `RenderTick`, stopped on `Unloaded`; a throw stops only that
  preview (ADR 0050).
- **Performance:** `OfSkills` runs the skills, classes and enemies checks on each typed key (a few dozen records);
  `OfTieredEffects` and `OfStatuses` rebind the catalog first (a pass over skills and items); the problem lines react to
  typed fields only (`.claude/rules/editor.md`).
- **Determinism and saves:** no Core rule changes; no save format change (decision 997).
