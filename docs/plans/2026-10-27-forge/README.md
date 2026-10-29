# The forge
Status: in progress

Split out of [the loot plan](../done/2026-10-27-loot/README.md) on 2026-10-27, when the developer approved it: materials, salvage and a bit of crafting (decisions 351-354), at a forge master NPC on the map (decision 353). Runs after the loot plan (done) and after [the affix stats plan](../done/2026-10-27-affix-stats/README.md): its A4a gives `GearRoll.Roll(item, luckPermille)` and `DropRoll.Roll(tables, luckPermille)`, and this plan builds on those signatures (the Forge's `GearRoll` members take no luck).

- Numbers: decisions 339, 351-354 and 362 (from the loot plan), 357, 358 and 369-372. No ADR (see "ADRs").
- Approved on 2026-10-29 with the developer's answers (357, 369-372) and the devil's advocate's edits: Core's `Forge` says what each action costs or gives and why it is refused (the screen only formats it); the crafting rules are checked from the loader once the items are loaded; the `openForge` step is wired through `EventPresenter`, `ExplorationScene` and `GameFlow`; the forge is reachable by hand right after the screen opens; slices split to stay under about 250 lines (M2 to M4, F1a to F1d, F2a to F2c).

## Goal
1. **Materials**: Scrap and Essence, their own tier and color (decision 339, 362: a muted teal), a Materials tab on the Inventory page, dropped by goblins (decision 371).
2. **Salvage**: an owned piece breaks into its rarity's materials (decision 354), refused unless the whole yield fits (decision 358).
3. **Upgrade and reroll** (decisions 351, 352): Common to Magic to Rare, one more affix each step (a crafted Rare has two; only drops reach three); every affix rolled again at the same tier. Paid with materials.
4. **At a forge master** (decisions 353, 372): an `openForge` event step opens the Forge screen, with its editor support, and an NPC on the start map runs it.

## Order, and why
1. **Phase 1: materials and crafting in Core (M1, M2, M3a, M3b, M4).** Materials first, so goblins drop them and the Inventory page shows them; then salvage, upgrade and reroll as Core rules with their tests; then the crafting numbers in `rules.json`, checked by the loader against the items.
2. **Phase 2: the forge on the map (F1a, E1, E2, N1).** A bare Forge screen, the event step that opens it, the editor support, then the forge master: from N1 on, every slice is checked by hand at the forge (the review's blocker 4).
3. **Phase 3: what the Forge shows (F1b, F1c, F1d).** The owned pieces, the detail column (affixes, materials), then the look.
4. **Phase 4: crafting at the Forge (F2a, F2b, F2c).** The actions with Core's costs and refusals, salvage behind a prompt, then upgrade and reroll.

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0030's pattern): yields and costs are `rules.json` `crafting`; every number is a placeholder (design.md, "Tunables").
- **Every "can it be done" rule is Core's** (`.claude/rules/game.md`): `Crafting/Forge` returns a `ForgeQuote` per action (what it moves, and its `ForgeRefusal` if refused); the screen formats it and never decides.
- **A refusal changes nothing and draws nothing**: every check runs before the first draw or the first material taken, so a refused action leaves the inventory and the random as they were.
- **One `GearRoll`** for drops and the Forge (`GameSession.GearRollFor`), its pool in id order, so a seed always draws the same affixes.
- **Pure presenter + thin view** (`docs/map.md`); one selection look (decision 252), every list through `SelectionMotion`; the Forge's shared pieces come from `Ui/`.
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x the class mechanics plan measured; Editor slices aim at about 200 lines.
- **Files near 300 lines:** new tests go in new files beside `EventPresenterTests` (305), `GameFlowTests` (321), `EventRunTests` (269); watched: `GameFlow` 246, `EventPresenter` 238, `GameSession` 237, `InventoryPage` 242, `ItemCatalogValidatorTests` 233, `ContentCatalogLoaderTests` 249, `EventStepViewModelTests` 244, `EventListViewModelTests` 237.
- **No save format change:** materials are stacks and crafted pieces are pieces (ADR 0029); saves already keep both. **No NuGet package.**

## Out of scope (follow-ups)
- **Crafting a worn piece** (decision 369): the Forge lists owned pieces only; taking one off first is the way for now.
- **Unique items** (decision 339): never a salvage yield nor an upgrade target here; the Forge refuses them until a plan makes them.
- **Gold, shops and prices** (question 222): crafting costs materials only.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: materials and crafting in Core
- [x] **M1. Materials: an item kind, their color, a Materials tab, Scrap and Essence that goblins drop** (Core + Data + content + Game, about 210 lines) → [slices/M1.md](slices/M1.md)
- [x] **M2. Salvage in Core: `Crafting/Forge`, its quotes and refusals** (Core, about 220 lines) → [slices/M2.md](slices/M2.md)
- [x] **M3a. Upgrade in Core: one more affix from the shared `GearRoll`** (Core + Simulations, about 250 lines) → [slices/M3a.md](slices/M3a.md)
- [ ] **M3b. Reroll in Core: every affix drawn again** (Core, about 120 lines) → [slices/M3b.md](slices/M3b.md)
- [ ] **M4. The crafting rules in content, checked against the items** (Data + content, about 240 lines) → [slices/M4.md](slices/M4.md)

### Phase 2: the forge on the map
- [ ] **F1a. A Forge screen that opens over the map and Cancel closes** (Game + text, raw 90, about 145 lines at 1.6x) → [slices/F1a.md](slices/F1a.md)
- [ ] **E1. The `openForge` event step opens the Forge and waits for it** (Core + Data + Game, about 210 lines) → [slices/E1.md](slices/E1.md)
- [ ] **E2. The editor shows and adds the `openForge` step; the Forge master preset** (Editor, about 110 lines) → [slices/E2.md](slices/E2.md)
- [ ] **N1. A forge master stands on the start map** (content + tests, about 70 lines) → [slices/N1.md](slices/N1.md)

### Phase 3: what the Forge shows
- [ ] **F1b. The Forge lists the owned pieces** (Game + text, about 165 lines) → [slices/F1b.md](slices/F1b.md)
- [ ] **F1c. The detail column: the highlighted piece's affixes and every material owned** (Game + text, about 150 lines) → [slices/F1c.md](slices/F1c.md)
- [ ] **F1d. The Forge's look: bars, columns, bands and its fade** (Game, raw 150, about 240 lines at 1.6x) → [slices/F1d.md](slices/F1d.md)

### Phase 4: crafting at the Forge
- [ ] **F2a. Confirm on a piece opens its actions, with Core's costs and refusals** (Game + text, about 200 lines) → [slices/F2a.md](slices/F2a.md)
- [ ] **F2b. Salvage, behind a prompt** (Game + text, about 130 lines) → [slices/F2b.md](slices/F2b.md)
- [ ] **F2c. Upgrade and reroll carried out; the cursor stays on the changed piece** (Game + text, about 150 lines) → [slices/F2c.md](slices/F2c.md)

## Decisions
From the loot plan (2026-10-27): 339 (Material is the materials' own tier and color), 351 (crafting is upgrade and reroll), 352 (reroll rolls every affix at once), 353 (a forge master NPC, an event step with editor support), 354 (Scrap and Essence; worn pieces can't be salvaged; salvage asks to confirm), 362 (the tiers' colors; Material a muted teal).
- **357. An upgrade when the pool has no affix left for the piece's kind is refused, greyed, and costs nothing** (answered 2026-10-29). Today's content can't reach it (every kind has at least three affixes), so its test uses a custom pool.
- **358. Salvage is refused, with nothing changed, unless the whole yield fits**: nothing is ever destroyed for less than it yields (clarified 2026-10-29: a stack that would overflow refuses it, not only a full one).
- **351, clarified (2026-10-29): "one more affix each step" means a crafted Rare has two affixes**; only drops reach three (Path of Exile's Regal Orb).
- **369. The Forge works owned pieces only**: worn ones aren't listed (consistent with 354). Crafting a worn piece is a follow-up.
- **370. Forge screen conventions:** a refused action is greyed with its reason (highest rarity, no affix left, nothing to reroll, short of materials, a full stack); the materials panel lists every material, 0 included, since the costs name them; the Forge opens like the menu (no fade to black, it fades itself in).
- **371. Essence also drops from goblins** at 5 % (placeholder), beside Rare salvage; salvage only would allow about one reroll per thirty fights. The goblin table: Scrap 20 % 1-2, Essence 5 %.
- **372. The forge master stands on the start map**, outdoors near where the player walks by, with the shared placeholder NPC sprite (ADR 0015).

## Open design questions (developer decides)
None open. Noted for the developer's review (consequences of the decisions above, not forks): a reroll is a fresh draw, so it may give back the same lines (decision 352, as Path of Exile's Chaos Orb); the materials panel lists materials in id order (the catalog keeps no file order).

## ADRs
None needed: the design follows ADR 0030's pattern (mechanisms in code, numbers in content) and ADR 0006's event steps (a step that waits for `ContinueEvent`, as dialogue does).
