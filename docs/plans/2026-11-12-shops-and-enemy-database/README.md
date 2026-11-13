# Shops and the enemy database
Status: approved, in progress

Drafted on 2026-11-12 from the developer's request (lightly cleaned up):

> "I want to be able to set up dialogue, quest, store and the smith. I don't know if it's a good idea but I will want to be able to also set up new monsters: import image, set up stats and maybe even strategy/behavior from the editor. This will need to grow with the game obviously: some stats might change, some may be removed and some added in the future. But I want to be able to start setting up the introduction and the first major quest line soon. [...] I want you to challenge these ideas: is it where it needs to be, should it be in code? How is it usually done by other software engineers? I see that as an 'in-house engine'."

Approved on 2026-11-12 with the developer's changes: the enemy tactics and phases forms (EN4, EN5, EN8) and the Encounters window (EC1-EC3) moved to "Later"; the class and skill editors plan drafted beside this one was dropped (backlog: balancing stays in JSON until the schema settles); the devil's advocate's EN1 filter rows added; every other question as recommended.

**Depends on** [`2026-11-12-character-sprites-and-npcs`](../2026-11-12-character-sprites-and-npcs/README.md): its CW1 (a save writes only the catalog files that changed, refusing one edited on disk), its SI1 (`Data/Content/CatalogProblems`, `ContentWriter.ImportArtImage`) and its SI4 (`Shell/StatusBand`, room in `MainWindow.xaml`). Run it after that plan.

Builds on ADR 0011 (content the only source of truth, the editor writes through Data), ADR 0012 and 0014 (editor, catalog undo), ADR 0027 (damage types), ADR 0030 (drop tables), ADR 0031 (shops and gold), ADR 0032 and 0033 (quests and conditions), ADR 0034 (enemy tactics and phases), ADR 0038 (art sources), the quests plan's Quests window (decision 394: the pattern this plan follows).

- Numbers: decisions 580-591, ADR 0045.
- Core: nothing new; the editor builds Core's records (`ShopDefinition`, `EnemyDefinition`). Data: `CatalogProblems.OfShops` and `OfEnemies`, the writer writes `shops.json` and `enemies.json`. No save format change.

## Goal
1. **Stores are set up in the editor.** Database > **Shops...**: each shop's id (typed while new), name and the items it sells in order, picked from the items that have a price. The shopkeeper preset's shop box then offers it.
2. **Quest NPCs in two clicks.** A **Quest giver** preset and a portrait picker on the dialogue step. Dialogue (speaker, portrait), the smith (forge master preset) and quest steps already exist.
3. **Monsters are set up in the editor.** Database > **Enemies...**: numbers, resistances by damage type, gold, drops, boss flag, and the art (battle sprite, portrait, attack effect) picked or imported. Tactics and phases stay hand-written in `enemies.json` for now (kept untouched by the window) and encounters in `encounters.json`.

## The challenge (the developer asked)
- **Is it where it needs to be?** Yes for numbers and choices, no for behavior. Enemy *numbers* and *which* tactic to use when are data: content and an editor. *What* a condition or an effect can do (a new `TacticConditionKind`, a new `EffectKind`) is a rule: Core code with its tests, the editor offering the closed list Core defines. That is how in-house JRPG engines are usually built (data-driven, verbs in code): RPG Maker's Database edits actors, classes, skills, items, enemies and troops as forms over fixed fields, its "action patterns" being our tactics; behaviors beyond the list are plugins (code). Scripting new behavior from the editor is what big engines afford; for one developer it doubles the work for every rule and loses the tests.
- **How do others build the forms?** (a) a hand-made form per data type (RPG Maker, most in-house tools); (b) a form generated from the type (Unity's and Godot's inspectors, by reflection or a schema); (c) a spreadsheet exported to JSON (common for balancing). Here the JSON already is the data and Data already validates it, so each is a view over the same records.
- **The answer (decision 580):** hand-written forms per catalog with two rules that make growth cheap: Data's validator is the one rule source, shown live as the problem line (`CatalogProblems`); rows over Core's closed lists are generated from the enum (one resistance row per `DamageType`). A field added to `EnemyDefinition` costs one property, one XAML row and one test, beside the Core, Data and content work it needs anyway. And for balancing many records at once, the first tool worth building is a read-only side-by-side table, not more per-record forms (backlog).

## Order, and why
1. **Phase 1: shops (SH1-SH3).** The smallest catalog first: it proves the pattern (the writer writes the file, `CatalogProblems` live, one undo step per OK, ids fixed once saved) on three fields, and the introduction's first town needs a store.
2. **Phase 2: quest NPCs (NP1, NP2).** Two small conveniences for the first quest line.
3. **Phase 3: enemies (EN1-EN3, EN6, EN7).** The file written by the editor first (relaid once), then the form's numbers, resistances and rewards, the art, the window.

Phases 1 and 2 don't depend on each other; EN6 needs NP2's `ArtFolders.ImagesIn`; EN1 needs SH1's `CatalogDraft.Database.cs`. Between EN1 and EN7 enemies change only in tests.

## Guiding rules
- **One rule source** (ADR 0045): every window's problem line is its own checks for what Data can't see (a typed id's shape, a picker left empty) and then `CatalogProblems`' message; no editor code copies a validator's rule.
- **A file the editor writes is laid out as the writer writes it** (`.claude/rules/content.md`): each writing slice relays its file once (a throwaway test running the writer, deleted after; the commit holds only the relay), with a test that the catalog loaded before and after is the same, then the identical-bytes test. After CW1 a save writes the file only when its content changed.
- **Undo covers every edit** (ADR 0014): each window's OK is one `CatalogDraft` step; an undo removing a shop a map's event opens is refused, naming the map.
- **Ids typed while new, fixed once in the draft; only new records removed** (decisions 581, 582).
- **Headless rules, thin windows**: rules with tests first, then the view with a hand check; Editor slices about 200 lines, tests counted as about as many lines as the code.
- **Agents never open bought art** (decision 438): tests import PNGs the tests write.
- **Files that must not grow past 300**: `Shell/MainWindow.xaml` (286 after plan 1) gains three menu lines (Database header renamed, Shops..., Enemies...: 288); `Shell/MainViewModel.cs`: new members in `MainViewModel.Database.cs` (SH3); `Catalogs/CatalogDraft.cs` (269): setters in `CatalogDraft.Database.cs` (SH1); `Shell/CombinedHistory.cs` (192): `RefusalOfShopUndo` its own method; test files past 250 take no new test (`ContentWriterTests` 254, `ContentCatalogLoaderTests` 272).

## Out of scope (follow-ups)
- Items, drop tables, affixes, party members, classes and skills as editor windows (the class and skill plan was dropped: backlog).
- Removing a saved shop or enemy from the editor (decision 581).
- Enemy battle sprite sheets (animated battlers): the battle draws one still picture (decision 100).
- New tactic condition kinds or effect kinds: Core code, each with its plan.
- The art source line written by the import (ADR 0038): still by hand.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when a slice is done and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: shops
- [x] ADR 0045: database editors (hand-written forms, Data's validators as the one rule source, files relaid once). Write it from the entry under "ADRs" below (decisions 580-582, 591).
- [x] **SH1. Shops save from the editor: `shops.json` relaid and written, `CatalogProblems.OfShops`, `CatalogDraft.SetShops`, an undo removing a shop an event opens refused** (Data + Editor, about 220 lines) → [slices/SH1.md](slices/SH1.md)
- [x] **SH2. The Shops window's rules: shops in id order, a shop's name and its priced items in order** (Editor, about 200 lines) → [slices/SH2.md](slices/SH2.md)
- [x] **SH3. The Shops window and the Database menu** (Editor, about 170 lines) → [slices/SH3.md](slices/SH3.md)

### Phase 2: quest NPCs
- [x] **NP1. The Quest giver preset: the quest offered while not started, a reminder while active** (Editor, about 150 lines) → [slices/NP1.md](slices/NP1.md)
- [x] **NP2. The dialogue step's portrait picked from the portraits folder** (Data + Editor, about 120 lines) → [slices/NP2.md](slices/NP2.md)

### Phase 3: enemies
- [ ] **EN1. Enemies save from the editor: `enemies.json` relaid and written, resistances in percents, every default-valued field left out, `CatalogProblems.OfEnemies`, `CatalogDraft.SetEnemies`** (Data + Editor, about 230 lines) → [slices/EN1.md](slices/EN1.md)
- [ ] **EN2. The Enemies window's rules: the list, Add and Duplicate, an enemy's numbers; tactics and phases kept as they are** (Editor, about 220 lines) → [slices/EN2.md](slices/EN2.md)
- [ ] **EN3. Resistances by damage type, gold, drop table and boss** (Editor, about 170 lines) → [slices/EN3.md](slices/EN3.md)
- [ ] **EN6. Enemy art: battle sprite, portrait and attack effect picked or imported** (Editor, about 180 lines) → [slices/EN6.md](slices/EN6.md)
- [ ] **EN7. The Enemies window: the list, Stats and Art tabs** (Editor, about 220 lines) → [slices/EN7.md](slices/EN7.md)

## Later (not in this plan; slice files kept as drafts)
- **EN4, EN5, EN8: the tactics and phases forms** ([EN4](slices/EN4.md), [EN5](slices/EN5.md), [EN8](slices/EN8.md)). Until then tactics and phases are written by hand in `enemies.json`, and the window keeps them as they are.
- **EC1-EC3: the Encounters window** ([EC1](slices/EC1.md), [EC2](slices/EC2.md), [EC3](slices/EC3.md)). The devil's advocate's lighter idea comes first if it comes: a "New encounter from these enemies" button in the Enemies window (an encounter of the chosen enemies, named, on the default background), since a map's settings already pick encounters (`Maps/Settings/EncounterChoice`). Until then a new monster reaches a map through a hand-written entry in `encounters.json`.

## Decisions
- 580: each catalog gets a hand-written window; rows over a Core enum are generated from the enum; the problem line ends with Data's message (`CatalogProblems`).
- 581: a window removes only the records added in its session; a saved record stays (maps, events, encounters or saves may name it).
- 582: an id is typed while the record is new and fixed once the draft holds it (`Objects/ObjectIds`' rule), as quests.
- 583: the Quests menu becomes **Database**, holding Quests..., Shops..., Enemies....
- 584: a shop's item picker lists only priced items, sorted by name with the id beside it.
- 585: the Enemies window has Duplicate: a new enemy copying the chosen one, its id typed.
- 586: the writer leaves an enemy's default-valued fields out of `enemies.json`: `dropTableId`, `gold`, an action's `skillId` and `when` while null; `isBoss` while false; `tactics` and `phases` while empty; a condition's `percent` while 0; so an enemy written by the editor reads like the hand-written ones.
- 587: resistances are written in whole percents by damage type, in `DamageType`'s order, a type at 0 left out.
- 588: the Quest giver preset makes two events: the first gated "quest not started" (a dialogue, then start the quest), the second gated "quest active" (a dialogue); the quest picked beside the preset button, the lines left to type.
- 589: the portrait box lists the PNGs of `content/art/portraits/` and still accepts a typed name.
- 590: imported enemy art is copied through Data's `ImportArtImage` into `battlers/`, `portraits/` or `effects/`; an effect strip that isn't whole square frames is refused at import.
- 591: each window applies its whole session as one undo step; a modal window needs no re-check of the draft under it.

## Open design questions (developer decides)
None left: every question was answered at approval.

## ADRs
- **0045, Database editors.** Options: (a) hand-written forms per catalog with enum-driven rows and Data's validators as the one rule source (chosen); (b) reflection-generated forms; (c) JSON-schema-generated forms (.NET's `JsonSchemaExporter`); (d) an external spreadsheet or database tool exporting the JSON. Decides decisions 580-582 and 591, that a file the editor writes is relaid once through the writer, and that the first balancing tool beyond the forms is a read-only side-by-side table.
