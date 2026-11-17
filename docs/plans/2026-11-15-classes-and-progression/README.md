# Classes and progression (editor push, roadmap row 12)
Status: approved, in progress

Drafted on 2026-11-15 from roadmap row 12 of [the editor push](../2026-11-15-editor-push/README.md) ("Class windows or
pages (stats, growth, skills, resources), skills learned by level, the experience table", its design.md section 3.11)
and the developer's words: "the only thing left after that... will be class edition and progression." Approved on
2026-11-15 by delegation from the developer, with the devil's advocate's review applied and the coordinator's two
calls: skills learned at a level are built (the approved roadmap names them; phase LV), and `new-game.json` gets its
page (phase NG). The developer was away: every game-design choice taken for him is marked "(default taken while the
developer was away; his to overturn)" and listed under "Defaults to confirm", the level rule first.

**Before this plan starts:** rows 6-11 of the push are done, among them row 7's skill editors (`CatalogProblems
.CheckSkills`, `CatalogDraft.SetSkills`, Core's `Battle/ClassBattleStats`) and row 11's mode bar through its stage 3
(the World and Database modes, `Workspaces/` with `IEditedCatalog`, `ICatalogEditing<T>`, `CatalogPage`, the page
recipe of its WS12, and the Database menu in `Shell/DatabaseMenu.xaml`). If the developer moves row 11 after this row,
the three page slices (CL4, PM4, XP3, and NG3) fall back to modal windows on ADR 0045's pattern; each says how.

## Goal
No hand-written JSON left for classes, party members, the new game and the experience table, and one new rule the
roadmap names:
1. **A Classes page** (Database > Classes): a class's name, description, base stats and growth per level with a
   read-only level table (each level's max HP, speed, accuracy and primaries, from Core), its skills in menu order each
   with the level it is learned at, and its battle resources with their gains and rules.
2. **Skills learned at a level** (ADR 0062): a class's skill entry may name a level; a member knows the skills at or
   below its level, learns the next ones on a level up, and the victory screen says "Omer learned Meteor."
3. **A Party members page**: a member's name, class, summary and battle art, with Import... beside each picture.
4. **A New game page**: the leader choices in order, the starting items and gold, the start map, cell and facing, the
   player's field sprite.
5. **An Experience page**: the experience each level needs, with the running total, a level added or removed at the
   end, and a fill from a curve.
6. `classes.json`, `party-members.json`, `new-game.json` and `rules.json` become editor-written, in the writer's layout.

## Order, and why
1. **Classes (CL1-CL5)**, the developer's "class edition": the file (CL1), one class as typed (CL2), the catalog and its
   step (CL3), the page (CL4), the Levels tab (CL5).
2. **Skills learned at a level (ADR 0062, LV1-LV3)** before the Skills tab, so CL6 builds the Level column from the
   start: the file shape first with every level 1 (LV1, nothing changes in play), then the member's rule (LV2), then
   the victory line (LV3).
3. **The rest of the Classes page (CL6-CL9):** skills with their levels, then the resources (rules, tab, the four
   optional rules).
4. **Party members (PM1-PM4)** then **the new game (NG1-NG3)**, which names members.
5. **Experience (ADR 0061, XP1-XP4)** last: `rules.json` is the one takeover with a real alternative (the ADR); the
   Levels tab and LV1's level check already read the draft's table.

Each phase is usable at its end. Each page slice comes after the slices that make its view model, so it is XAML, a
registration, a menu entry and a hand check.

## Guiding rules
- **Edit what the game does; one new rule, named:** every number a page shows is computed by Core as the game computes
  it (`ClassGrowth`, `MemberSheet` through `Party/ClassSheet`, `ExperienceTable`). The one rule this plan adds is the
  roadmap's skills learned at a level (ADR 0062), absent levels meaning level 1 so nothing changes in play until one is
  typed. A member's starting gear stays out (decision 960).
- **Data's rules stay Data's:** each page's problem line is its own typed boxes first, then the loader's message through
  `CatalogProblems` (ADR 0045); no editor rule copies a content rule.
- **The page recipe** (row 11's WS12, decisions 694 and 942): the catalog view model implements `IEditedCatalog`, the
  editing view model `ICatalogEditing<T>`; the session is one `CatalogDraft` step on Apply or leave (ADR 0060); ids
  typed while new and fixed once the draft holds them (decision 582); only records added in the session are removed
  (decision 581).
- **Tests on repository content never pin today's values** (`.claude/rules/tests.md`): the relay tests compare the
  catalog loaded before and after; editor tests build their classes and members from `Party/ClassFixtures` and
  `Party/PartyMemberFixtures`, never from a repository record's numbers.
- **Sizes:** Editor slices about 200 lines with tests at about 10 lines each; Core and Data slices about 250 with tests
  at about 13; Game slices at the 1.6x a new view costs. Relaid JSON is counted apart.
- **Files near 300:** with the mode bar, the entries go to `Shell/DatabaseMenu.xaml`, the page lines to
  `MainViewModel.Workspaces.cs` and `MainWindow.Database.cs`; neither `MainWindow.xaml` (294 on 2026-11-15) nor
  `MainViewModel.cs` (286) grows here. Each page slice recounts the files it touches first; one that would pass 300
  makes the move in that slice (decision 962). Counted on 2026-11-15: `Catalogs/CatalogDraft.cs` 272 (untouched: the
  steps go to the new `CatalogDraft.Party` partial and `.Database`, 118); `Shell/CombinedHistory.cs` 221 (+15, PM3);
  `Data/Content/CatalogProblems.cs` 130, about 190 after row 7 (CL1 starts the `CatalogProblems.Party` partial);
  `Core/Party/PartyMember.cs` 241 (LV2 puts the known-skills rule in the new `Party/SkillsByLevel`, so the member
  stays near its size); `Game/Battle/VictoryScreenView.cs` 277 (LV3 draws its lines in the new
  `VictoryLearnedSkillsView`, +about 4 here).
- **Test files, globbed on 2026-11-15:** no `tests/Jrpg.Editor.Tests/Party/` or `Progression/` folder; no
  `ContentWriterClassTests`, `ContentWriterPartyMemberTests`, `ContentWriterNewGameTests`, `ContentWriterRulesTests`,
  `CatalogProblemsPartyTests`, `CatalogProblemsRulesTests`, `CatalogDraftClassTests`, `CatalogDraftPartyMemberTests`,
  `CatalogDraftNewGameTests`, `CatalogDraftExperienceTests`, `CatalogUsagePartyMemberTests`,
  `CombinedHistoryPartyMemberTests`, `ClassSheetTests`, `ClassDefinitionTests`, `PartyMemberSkillLevelTests`,
  `ClassSkillLevelCheckTests`, `ContentCatalogLoaderClassSkillTests`, `VictoryScreenLearnedSkillTests`,
  `RecordChoiceTests`, `NewGameViewModelTests`, `ExperienceTableFillTests`.
  Extended: `ContentWriterRepositoryTests` (81), `ContentWriterRenameTests` (NG1), `Battle/BattleArtChoicesTests` (80,
  PM4), `PartyMemberTests` and `ClassCatalogValidatorTests` (their `SkillIds` sites, LV1).
- **Each page joins `.claude/scripts/open-editor-windows.ps1`'s default list** in its slice ("shown as a page"); hand
  checks rely on the event window plan's EW0 ("no binding-error line").

## Out of scope
- **A member's starting gear:** members join with nothing worn; the new game's starting items are the party's
  inventory. A later plan (design.md, section 6). Decision 960.
- **The rest of `rules.json`** (party size, stat conversions, walking, loot rarities, sale share, crafting): written by
  the editor from XP1 on, but no form edits them here; a later rules page costs only its boxes (ADR 0061).
- **Per-class experience curves**, a level cap other than the table's length, class art or icons: none exist.
- **A skill forgotten at a level, or a class change:** not asked for.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).
`slices/CL0.md` is superseded (decision 962): delete it.

### Phase 1: classes
- [x] **CL1. `classes.json` is written by the editor, in the writer's layout** (Data + content, about 200 lines plus the
  relaid file) → [slices/CL1.md](slices/CL1.md)
- [x] **CL2. A class's base or growth as eight typed boxes** (Editor, about 250 lines; split, the class itself is CL2b)
  → [slices/CL2.md](slices/CL2.md)
- [x] **CL2b. One class as typed: its id, name, description and the two sets of boxes** (Editor, about 230 lines; split
  from CL2) → [slices/CL2b.md](slices/CL2b.md)
- [x] **CL3. The classes as a catalog: Add, Duplicate, Remove while new** (Editor, about 230 lines; split, the undo step
  is CL3b) → [slices/CL3.md](slices/CL3.md)
- [x] **CL3b. The classes' undo step: the session applied as one `SetClasses` step** (Editor, about 160 lines; split
  from CL3) → [slices/CL3b.md](slices/CL3b.md)
- [x] **CL4. Database > Classes: the page with its Stats tab** (Editor, about 50 lines plus about 120 of XAML) →
  [slices/CL4.md](slices/CL4.md)
- [x] **CL5. The Levels tab: a class's numbers at every level, through Core's `ClassSheet`** (Core + Editor, about 230
  lines; split, the tab on the page is CL5b) → [slices/CL5.md](slices/CL5.md)
- [x] **CL5b. The Levels tab on the page: the chosen class's rows as typed** (Editor, about 160 lines; split from CL5) →
  [slices/CL5b.md](slices/CL5b.md)

### Phase 2: skills learned at a level
- [x] ADR 0062, skills learned at a level (amends ADR 0020). Write it from decisions 959, 963-966 and 970 and design.md,
  section 5 (the options: an optional level on each skill entry, chosen; a `skillLevels` map beside `skillIds`; a
  separate level-up table). The developer delegated approval; the rule is the first of "Defaults to confirm".
- [ ] **LV1. A class's skill entry may name a level: the file shape, its checks, every level 1** (Core + Data + content,
  about 240 lines) → [slices/LV1.md](slices/LV1.md)
- [ ] **LV2. A member knows the class skills at or below its level, and learns on a level up** (Core, about 220 lines)
  → [slices/LV2.md](slices/LV2.md)
- [ ] **LV3. The victory screen says "Omer learned Meteor."** (Game + content, about 200 lines) →
  [slices/LV3.md](slices/LV3.md)

### Phase 3: the rest of the Classes page
- [ ] **CL6. The Skills tab: the class's skills in menu order with the level each is learned at** (Editor, about 230
  lines) → [slices/CL6.md](slices/CL6.md)
- [ ] **CL7. One resource as typed: amounts and gains** (Editor, about 210 lines) → [slices/CL7.md](slices/CL7.md)
- [ ] **CL8. The Resources tab** (Editor, about 60 lines plus about 120 of XAML) → [slices/CL8.md](slices/CL8.md)
- [ ] **CL9. A resource's rules: Will scaling, backfire, strain and dominance** (Editor, about 210 lines) →
  [slices/CL9.md](slices/CL9.md)

### Phase 4: party members
- [ ] **PM1. `party-members.json` is written by the editor** (Data + content, about 130 lines plus the relaid file) →
  [slices/PM1.md](slices/PM1.md)
- [ ] **PM2. One member as typed and the members as a catalog** (Editor, about 220 lines) →
  [slices/PM2.md](slices/PM2.md)
- [ ] **PM3. The members' undo step, refused while a map recruits or requires an added member** (Editor, about 170
  lines) → [slices/PM3.md](slices/PM3.md)
- [ ] **PM4. Database > Party members: the page with its pictures and imports** (Editor, about 70 lines plus about 120
  of XAML) → [slices/PM4.md](slices/PM4.md)

### Phase 5: the new game
- [ ] **NG1. `new-game.json` joins the catalogs' write and stamps** (Data, about 150 lines) →
  [slices/NG1.md](slices/NG1.md)
- [ ] **NG2. The new game as typed: leader choices, starting items, gold, start, sprite; its undo step** (Editor, about
  230 lines) → [slices/NG2.md](slices/NG2.md)
- [ ] **NG3. Database > New game: the page** (Editor, about 40 lines plus about 110 of XAML) →
  [slices/NG3.md](slices/NG3.md)

### Phase 6: experience
- [ ] ADR 0061, `rules.json` written whole by the editor. Write it from decisions 943 and 955 and design.md, section 4
  (the options: the whole file, chosen; the table in its own `experience.json`; a patch of the `experience` node
  keeping the hand layout), with the stamps as the reason the whole-file write is safe (decision 559). The developer
  delegated approval; the choice is in "Defaults to confirm".
- [ ] **XP1. `rules.json` is written by the editor, every section in the writer's layout** (Data + content, about 140
  lines plus the relaid file; about 40 more if the rarity keys need their converter) → [slices/XP1.md](slices/XP1.md)
- [ ] **XP2. The experience table as typed, one undo step** (Editor, about 220 lines) → [slices/XP2.md](slices/XP2.md)
- [ ] **XP3. Database > Experience: the page** (Editor, about 30 lines plus about 80 of XAML) →
  [slices/XP3.md](slices/XP3.md)
- [ ] **XP4. Fill from a curve: a first amount and a growth percent** (Editor, about 190 lines) →
  [slices/XP4.md](slices/XP4.md)

## Decisions

Numbers 941-970. "(default ...)" marks a game-design choice taken while the developer was away; his to overturn.

Plan:
- **941.** Six phases: classes (CL1-CL5), skills learned at a level (ADR 0062, LV1-LV3), the rest of the Classes page
  (CL6-CL9), party members (PM1-PM4), the new game (NG1-NG3), experience (ADR 0061, XP1-XP4). Each is usable at its end.
- **942.** The editors are pages of the mode bar's Database mode (row 11, decision 694), on its WS12 recipe: Classes,
  Party members, New game and Experience under the `Characters` group after Character sprites, each a
  `Database > <Title>` entry in `Shell/DatabaseMenu.xaml` (default taken while the developer was away; his to overturn:
  the group). Their rules live in `Editor/Party/` (classes, resources, members), `Editor/NewGame/` (the new game) and
  `Editor/Progression/` (the experience table). If row 11 moves after this row, the page slices build modal windows on
  ADR 0045's pattern instead; the view models and their tests are the same either way.
- **943.** No save format change (`formatVersion` stays): saves keep a member's id, level, experience, HP, gear and
  lasting amounts by resource id; classes, members, the new game and the table are read at load, and the skills a
  member knows are derived from its class and level (decision 966). A lowered max level is repaired by
  `SaveGameRepair.RepairLevel`; a lasting amount whose resource left the class is ignored by
  `PartyMember.RestoreProgress`. Core changes: LV1-LV2's rule and CL5's `Party/ClassSheet` reading.

Classes:
- **944.** `classes.json` is editor-written: sorted by id, in the writer's layout, relaid once (ADR 0045). A resource's
  `maxFromSpeed`, `backfire`, `strain`, `dominance` and `willScaling` are left out while null, `isLasting` while
  false, `max` and `startsAt` while 0, and each gain while 0. The classes are checked once per save, by
  `CatalogProblems.CheckClasses(classes, skills)`, which CL1 extracts from row 7's `CheckSkills` and which `CheckSkills`
  calls; the classes, members and new game checks live in the `CatalogProblems.Party` partial.
- **945.** The Classes page lists the classes in id order, the added ones after; Add starts from the tunable blank,
  Duplicate copies the chosen class as typed with its skills and resources, its id left to type (decision 585's
  pattern); only an added class is removed (decision 581). One `CatalogDraft.SetClasses` step with no undo refusal:
  only members name a class, and catalog steps undo in order. The skills and resources no tab edits yet are carried
  silently.
- **946.** The problem line is the chosen class's typed boxes first ("Base max HP must be a whole number"), then Data's
  message for the whole list (`CatalogProblems.OfClasses`).
- **947.** Base and growth are the same eight boxes (max HP, speed, accuracy, the five primaries), one `ClassStatsBoxes`
  type used twice.
- **948.** The Levels tab shows, for each level from 1 to the draft's max level: the experience to the next, max HP as
  a member of the class has it with nothing worn, speed, accuracy and the five primaries, from Core's
  `Party/ClassSheet.At(class, level, rules)`, the sheet row 7's `Battle/ClassBattleStats.At` then maps (one copy of the
  rule). While a box can't be read, or a level's number would pass the largest whole number, the tab shows that
  problem instead of rows.
- **949.** The Skills tab lists the class's skills in menu order (ADR 0020) by name and id, each with the level it is
  learned at (ADR 0062); a skill is added after the chosen one from every skill of the draft not already listed;
  removed; moved. (default taken while the developer was away; his to overturn) The picker offers every skill, even one
  using a resource the class lacks: Data's message names it, so a skill can be added before its resource.
- **950.** The Resources tab lists the class's resources in order: Add (the tunable blank), Remove (any resource; Data
  refuses while a class skill uses it, and saves are safe by decision 943), Move. A resource's id is typed while new and
  fixed once the draft holds it (skills name it); its name, color (`#RRGGBB` with a swatch), lasting box, amounts (a max
  and a start, or from speed with the speed per charge) and its eight gains.
- **951.** Each resource rule (Will scaling, backfire, strain, dominance) is a box that turns it on with its numbers;
  off writes nothing. Strain's and dominance's other resource is chosen from the class's resources as typed; Data's
  rules stay Data's messages.
- **1011.** (CL1) A resource whose eight gains are all 0 leaves `gains` out of `classes.json` too, as the hand-written
  druid's Growth and Decay did, so the writer never writes `"gains": {}`.

Party members:
- **952.** `party-members.json` is editor-written: sorted by id, relaid once, checked against the draft's classes.
- **953.** The Party members page lists the members in id order, the added ones after; Add and Remove while new.
  (default taken while the developer was away; his to overturn) No Duplicate: a member is one character. Name; class
  chosen by name; summary in a wrapping box; portrait, battle sprite and attack effect as editable boxes offering the
  art folders' files (decision 589), marked when missing (decision 442), the portrait and battle sprite previewed;
  Import... beside each copies PNGs through Data (a GIF strip has no use for a member, who has no idle loop).
- **954.** An undo removing a member that a map's event recruits or names as its leader gate (`onlyWhenLeaderId`) is
  refused, naming the map, as for shops (decision 581). The new game names members too, but its step comes after the
  member's (catalog steps undo in order), so it needs no refusal.

Experience:
- **955.** `rules.json` is editor-written whole (ADR 0061): every section in the writer's layout, relaid once. It is
  safe because of the stamps (decision 559): a draft whose rules are unchanged never rewrites the file, a hand edit made
  while the editor holds a changed table makes Save all refuse (`CatalogFileStamps.IsEditedOnDisk`), and the content
  rule already asks every content file for the writer's layout. This plan edits only the experience table.
- **956.** The Experience page shows one row per level step, "Level 4 to 5", with its experience box and the total to
  reach that level, read-only; Add level appends a row copying the last one's amount; Remove last level is greyed at one
  row (Data needs one). One `CatalogDraft.SetExperienceTable` step with no undo refusal (decision 943's repair); its
  problem line includes LV1's check that no class learns a skill past the max level.
- **957.** Fill from curve: a first amount, a growth percent (above -100) and a max level make the rows: the first,
  then each the one before times (100 + percent) / 100, rounded down. Fill is greyed, with the reason on the line, while
  a row would pass the largest whole number or fall below 1. Fill replaces the rows; Revert throws the session away.
  An authoring tool: the game keeps reading the list (today's table is exactly first 100, 20 %, max level 21).
- **958.** (default taken while the developer was away; his to overturn) The fill makes 2 to 99 levels, the genre's
  usual cap; the table typed by hand has no cap but Data's.

Skills learned at a level (ADR 0062):
- **959.** Built, as the approved roadmap names it: a class's skill entry is `{ "skillId", "level" }`, the level a
  whole number from 1, left out of the file at 1 (`ClassDefinition.Skills`, a list of `ClassSkill`); `skillIds` goes,
  and `ClassDefinition.SkillIds` stays as a computed reading of every entry's id in order, so its readers don't change.
- **963.** (default taken while the developer was away; his to overturn) A member knows each class skill whose level
  is at or below its own, in the class's order; a level up adds the skills of the levels reached; a member recruited
  or loaded at a higher level knows every skill up to it. An entry without a level is known from level 1, so every
  class plays as today until a level is typed.
- **964.** (default taken while the developer was away; his to overturn) The victory screen names each skill learned:
  "Omer learned Meteor.", one line per skill, under the experience bars, shown once that member's bar has reached the
  level (`battle_skill_learned` in `ui.json`).
- **965.** Data refuses a skill level below 1 or past the experience table's max level (it could never be learned),
  naming the class and the skill; the loader checks it once the rules are read (`ClassSkillLevelCheck`), and the
  Classes and Experience pages' problem lines run it.
- **966.** No save change for the rule: the skills a member knows come from its class and level at recruit, on a
  level up and at load.
- **970.** The game's readers of a member's skills (the battle's Skills menu, the Status page, the leader profile) read
  the known ones (`PartyMember.Skills`); the readers of a class's whole list (Data's checks, row 7's balance table and
  "Learned by" line) read `ClassDefinition.SkillIds`.

Shared and new game:
- **960.** A member's starting gear is not built: members join with nothing worn, and no rule says otherwise.
- **961.** `new-game.json` is editor-written (reversing the draft's "stays hand-written"): it joins the catalogs' check,
  write and stamps; the map rename's write of it shares the same serialized bytes and renews its stamp.
- **962.** No up-front move of the shell's hot files: each page slice recounts the files it touches and, if one would
  pass 300, makes the move in that slice. The draft's CL0 is superseded.
- **967.** One shared choice record for the boxes that offer records of a catalog: `Catalogs/RecordChoice(Id, Name)`
  with its label "Fireball (fireball)" and "(missing) fireball" for an id the draft lacks (CL6), reused by the members'
  Class box (PM2) and the new game's boxes (NG2). The earlier one-catalog copies (`Loot/DropItemChoice`,
  `Trade/PricedItemChoice`, row 7's `Skills/StatusChoice`, row 8's `Battle/TacticSkillChoice`) each carry an extra (a
  price, a line, the attack entry) and stay; the backlog notes them for when next touched.
- **968.** The New game page: the leader choices in the order the leader screen shows them (members picked by name,
  added after the chosen one, moved, removed; at least one, Data's rule); the starting items as rows of an item and a
  quantity; the starting gold; the start map, its x and y (decimal cells, finite) and facing; the player's field sprite
  from the draft's field sprites. One `CatalogDraft.SetNewGame` step.
- **969.** The New game page's problem line is its typed boxes first, then Data's message (`CatalogProblems.OfNewGame`:
  the new game's own rules, its members, items, map and cell, and a field sprite with a record).

## Defaults to confirm
Taken while the developer was away, each his to overturn, the mechanic first:
1. **963, 959. Skills learned at a level:** each skill entry may name a level; a member knows the skills at or below
   its level, a recruit at a higher level knows all before it, and an entry without a level is known from level 1 (so
   nothing changes until he types one). Not taken: a separate level-up table, or skills bought or found.
2. **964. The victory line's wording:** "Omer learned Meteor.", one line per skill, under the bars.
3. **ADR 0061, `rules.json` written whole** (955): its layout changes once (one field per line). Not taken: the table in
   its own `experience.json`.
4. **958. The curve fill makes at most 99 levels.**
5. **942. The four pages under the Characters group.**
6. **953. No Duplicate on the Party members page.**
7. **949. The skill picker offers every skill,** Data naming the one whose resource the class lacks.

## Open design questions (developer decides)
- None blocking. For later plans: a member's starting gear (decision 960); per-class experience curves; the rest of
  `rules.json` as a page.

## Notes for other rows
- **Row 7 (skills editors):** CL1 extracts `CatalogProblems.CheckClasses(classes, skills)` from its `CheckSkills`, which
  then calls it; CL3 rewrites its `CatalogDraft.SetSkills` summary ("classes and enemies name skills but the editor
  writes neither") now that the editor writes classes; CL5 makes its `ClassBattleStats.At` map `Party/ClassSheet.At`;
  LV1 keeps `ClassDefinition.SkillIds` for its balance table and "Learned by" line (decision 970).
- **Row 9 (items):** the sale share lives in `rules.json` `trade`; from XP1 on it is editor-written, so a box costs only
  the form.
- **Row 11 (mode bar):** decision 942; this plan adds four pages to the Characters group.

## ADRs needed
- **0062, skills learned at a level** (before LV1), amending ADR 0020: decisions 959, 963-966, 970; options in
  design.md, section 5. Row 7 left ADR 0062 unused (its decision 995).
- **0061, `rules.json` written whole by the editor** (before XP1): decisions 943, 955; options and costs in design.md,
  section 4.
