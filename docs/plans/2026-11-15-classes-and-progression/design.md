# Classes and progression: design

The README holds the order, the decisions and the defaults. This file holds the tunables (section 1), what exists
(section 2), the structure (section 3), the `rules.json` choice behind ADR 0061 (section 4), skills learned at a level
behind ADR 0062 (section 5), the file layouts and what is left for later (section 6) and the risks (section 7).

## 1. Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| New class | `Party/ClassViewModel` (`_blank`) | name and description empty, base max HP 1, every other base and growth number 0, no skills, no resources | Add's starting record: the validator's lowest legal numbers, as the Enemies page's blank (decision 585's pattern). |
| New skill entry's level | `Party/ClassViewModel.Skills` (CL6) | 1 | What Add writes in the Level box: known from the start, as every entry today. |
| New resource | `Party/ClassResourceViewModel` (`_blank`) | id and name empty, color `#FFFFFF`, max 1, starts at 0, no gains, no rules; speed per charge 1 if From speed is ticked (decision 1015) | Add's starting resource. |
| New member | `Party/PartyMemberViewModel` (`_blank`) | every text empty, no class | Add's starting member. |
| New starting item row | `NewGame/StartingItemRow` | no item, quantity 1 | Add's row on the New game page. |
| Added level | `Progression/ExperienceTableViewModel` | the last row's amount | What Add level puts in the new last row. |
| Curve fill's boxes | `Progression/ExperienceCurveBoxes` | first: the table's first amount; growth 20 %; max level: the table's | What the fill boxes show when the page opens; 20 % is today's table's growth (decision 957). |
| Curve fill's level range | `Progression/ExperienceCurve.MostLevels` | 2 to 99 | Decision 958 (default). |
| Picture previews | `Party/PartyMemberCatalogPage` | 160 px wide, decoded at that width (`SpriteImageFile.LoadAtWidth`) | The portrait and battle sprite beside their boxes; party battlers are 1024 px paintings. |
| Victory line | `content/text/ui.json` `battle_skill_learned` | "{0} learned {1}." | Decision 964 (default wording). |

## 2. What exists (2026-11-15, with rows 6-11 assumed done)

- **Core, `Party/`:** `ClassDefinition(Id, Name, Description, Base, Growth)` with `SkillIds` and `Resources` (init,
  empty by default); `ClassStats(MaxHp, Speed)` with `Primaries` and `Accuracy`; `ClassGrowth.StatsAt(class, level)`
  (`base + growth × (level − 1)`, ADR 0017); `MemberSheet.Of(atLevel, worn, weaponDamage, rules)`;
  `PartyMemberDefinition` (id, name, class id, summary, portrait, battle sprite, attack effect); `PartyMember.Recruit`
  (level 1, every class skill in order, ADR 0020), `GainExperience` (steps), `RestoreProgress`. Row 7 adds
  `Battle/ClassBattleStats.At(class, level, rules)`: `MemberSheet.Of(ClassGrowth.StatsAt(...), StatBonuses.None, null,
  rules)` mapped to battle stats.
- **Core, `Resources/`:** `ResourceDefinition(Id, Name, Max = 0, StartsAt = 0)` with `Gains` (eight ints),
  `MaxFromSpeed`, `Backfire`, `Color`, `IsLasting`, `Strain`, `Dominance`, `WillScaling`.
- **Core, `Progression/` and `Battle/`:** `ExperienceTable(ExperienceToNextLevel)`, `MaxLevel` one past the list, in
  `GameRules.Experience`; `content/rules.json` `experience` has 20 entries, each the one before times 1.2 rounded down
  from 100. `Battle/ExperienceGain(Member, Amount, Steps, IsAtMaxLevel)` feeds the victory screen.
- **Data:** the loader reads `classes.json` (`ClassCatalogValidator`), `party-members.json`, `rules.json`
  (`GameRulesValidator`, then `CraftingRulesValidator` after the items) and `new-game.json` (`NewGameValidator`, then
  `FieldSpriteReferenceCheck`). Row 7's SK1 made `ClassesFile` internal and runs the class validator over the catalog's
  classes inside `CheckSkills`. The writer writes `new-game.json` only from `RenameMap`, unstamped. `classes.json` and
  `rules.json` are laid out by hand; `party-members.json` and `new-game.json` already one field per line.
- **Saves** keep each member's id, level, experience, HP, worn pieces and lasting amounts by resource id;
  `SaveGameRepair.RepairLevel` clamps a level to the table's max.
- **Editor (rows 7 and 11 done):** the Database mode's pages (`Workspaces/`: `IEditedCatalog` with `ChosenKey` and
  `Choose`, `ICatalogEditing<T>`, `CatalogPage.Of(title, group, editing)` in `Shell/MainViewModel.Workspaces.cs`,
  `DatabasePageViews.Register<T>` in `Shell/MainWindow.Database.cs`, `CatalogGroup.Characters`, the entries in
  `Shell/DatabaseMenu.xaml` bound to `ShowDatabasePageCommand`); the catalog view model pattern (`Problem`, `CanApply`,
  `ToX()`, Add, Duplicate, Remove while new); `TypedNumber`, `RecordListProblem`, `Events/ListOrder`, `ObjectIds`;
  `Battle/BattleArtChoices`; `MainViewModel.ImportArtImages`; row 7's `CatalogDraft.Skills` partial.

## 3. Structure

### New types

Core:

| Type | Slice | Responsibility |
|---|---|---|
| `Party/ClassSheet` (public static) | CL5 | A member's sheet for a class at a level with nothing worn. |
| `Party/ClassSkill` (public sealed record) | LV1 | One skill of a class with the level it is learned at. |
| `Party/SkillsByLevel` (internal sealed) | LV2 | A class's skills resolved from the catalog with the level each is learned at. |
| `Battle/LearnedSkill` (public sealed record) | LV2 | One skill a member learned in a battle's experience, with the level it came at. |

Data: `Party/ClassSkillLevelCheck` (LV1): every class skill's level within the experience table, once both are read.

Editor, `Editor/Party/` (no type is named `Party`, CA1724):

| Type | Slice | Responsibility |
|---|---|---|
| `ClassStatsBoxes` | CL2 | One `ClassStats` as eight typed boxes, with the first box that isn't a whole number. |
| `ClassViewModel` | CL2 | One class as the Classes page types it, carrying from its record what no tab edits yet. |
| `ClassCatalogViewModel` | CL3 | Every class on the page, Add, Duplicate, Remove while new, and the problem line. |
| `ClassEditingViewModel` | CL3 | Starts the Classes page from the draft and applies its session as one step. |
| `ClassCatalogPage` | CL4 | The Classes page: the list left, the chosen class's tabs right. |
| `ClassLevelPreview` | CL5 | A class's rows at each level of the table, or why they can't be shown. |
| `LevelPreviewRow` | CL5 | One level's numbers in the Levels tab. |
| `ClassSkillRow` | CL6 | One skill of the class as typed: the skill and its level box. |
| `ClassResourceViewModel` | CL7 | One resource as the Resources tab types it. |
| `ResourceGainsBoxes` | CL7 | A resource's eight gains and heavy-hit share as typed boxes. |
| `HexColorBrushConverter` | CL8 | A `#RRGGBB` text to a brush for the swatch, transparent while unreadable. |
| `WillScalingBoxes`, `BackfireBoxes`, `StrainBoxes`, `DominanceBoxes` | CL9 | One optional resource rule each: whether it is on, and its boxes. |
| `PartyMemberViewModel` | PM2 | One member as the Party members page types it. |
| `PartyMemberCatalogViewModel` | PM2 | Every member on the page, Add, Remove while new, the class choices and the problem line. |
| `PartyMemberEditingViewModel` | PM3 | Starts the Party members page from the draft and applies its session as one step. |
| `PartyMemberCatalogPage` | PM4 | The Party members page: the list left, the chosen member's boxes and pictures right. |

`Editor/Catalogs/RecordChoice` (CL6): one record a box offers, its id and name (decision 967).

`Editor/NewGame/`:

| Type | Slice | Responsibility |
|---|---|---|
| `NewGameViewModel` | NG2 | The new game as the New game page types it. |
| `StartingItemRow` | NG2 | One starting item as typed: the item and its quantity box. |
| `NewGameEditingViewModel` | NG2 | Starts the New game page from the draft and applies its session as one step. |
| `NewGamePage` | NG3 | The New game page. |

`Editor/Progression/`:

| Type | Slice | Responsibility |
|---|---|---|
| `ExperienceRowViewModel` | XP2 | One level step's experience as typed, with its running total. |
| `ExperienceTableViewModel` | XP2 | The table as the Experience page types it: rows, Add level, Remove last level, the problem line. |
| `ExperienceEditingViewModel` | XP2 | Starts the Experience page from the draft and applies its session as one step. |
| `ExperienceTablePage` | XP3 | The Experience page. |
| `ExperienceCurve` | XP4 | The fill's rule: a first amount and a growth percent into a table's rows, or why it can't. |
| `ExperienceCurveBoxes` | XP4 | The fill's three typed boxes and the rows they make. |

Shell and Catalogs: `Catalogs/CatalogDraft.Party` (`SetClasses`, `SetPartyMembers`, `PartyMembersRemovedByUndo`,
`SetNewGame`), `CatalogDraft.Database` (`SetExperienceTable`), `CatalogUsage.UsersOfPartyMember`,
`Shell/CombinedHistory.RefusalOfPartyMemberUndo`; per page one `CatalogPage.Of` line, one `Register<T>` line and one
`DatabaseMenu.xaml` entry.

### Data changes

- `Data/Content/CatalogProblems.Party.cs` (new partial, CL1): `OfClasses`/`CheckClasses(classes, skills)` (extracted
  from row 7's `CheckSkills`, which calls it), `CheckClassSkillLevels` (LV1), `OfPartyMembers`/`CheckPartyMembers`
  (PM1), `OfNewGame`/`CheckNewGame` (NG1). `CatalogProblems.Rules.cs` (new partial, XP1): `OfRules`/`CheckRules`.
- `ContentWriter.Catalogs`: `classes.json`, `party-members.json`, `new-game.json` and `rules.json` join
  `SerializedCatalogs`; their checks join `CheckCatalogs` (the classes through `CheckSkills`, once).
- `AbsentWhenNullFilter`, `AbsentWhenFalseFilter`, `AbsentWhenZeroFilter`: the resource rows (CL1);
  `AbsentWhenOneFilter`: `(ClassSkill, "level")` (LV1).

### Folder dependencies (Editor)

| Folder | Reads | Never reads |
|---|---|---|
| `Party` | `Catalogs` (`CatalogDraft`, `TypedNumber`, `RecordListProblem`, `RecordChoice`), `Events` (`ListOrder`), `Objects` (`ObjectIds`), `Battle` (`BattleArtChoices`, PM4), `Sprites` (`SpriteImageFile`, the page only), `Workspaces` (`IEditedCatalog`, `ICatalogEditing<T>`); Core `Party`, `Stats`, `Resources`, `Progression`, `Skills`; Data `CatalogProblems` | `Shell`, `NewGame`, `Progression` |
| `NewGame` | `Catalogs`, `Events`, `Workspaces`; Core `Content`, `Geometry`, `Items`; Data | `Shell`, `Party` |
| `Progression` | `Catalogs`, `Workspaces`; Core `Progression`, `Content`; Data | `Shell`, `Party`, `NewGame` |
| `Battle` | (unchanged) | `Party` (PM4 changes `BattleArtChoices.MissingOf` to take three file names) |
| `Catalogs` | Core records only | `Party`, `NewGame`, `Progression`, `Workspaces` |
| `Shell` | the three folders (pages, entries) | |

Core: `Party/ClassSheet` reads `Stats`; `Battle/ClassBattleStats` reads `Party/ClassSheet` (Battle already reads Party);
`Battle/LearnedSkill` reads `Skills`. No new edge.

### Replacement mapping

| Hand-written or copied today | Replaced by |
|---|---|
| `classes.json` base, growth, name, description | Classes page, Stats tab (CL2-CL4) |
| `classes.json` `skillIds` | `skills` with levels (LV1), the Skills tab (CL6) |
| `classes.json` `resources` | Resources tab (CL7-CL9) |
| `party-members.json` | Party members page (PM2-PM4) |
| `new-game.json` (hand, and the rename's unstamped write) | New game page, stamped like the catalogs (NG1-NG3) |
| `rules.json` `experience` | Experience page (XP2-XP4) |
| `ClassBattleStats.At`'s inline `MemberSheet.Of(ClassGrowth.StatsAt(...))` | `Party/ClassSheet.At` (CL5) |

## 4. Who writes `rules.json` (ADR 0061)

The experience table lives in `rules.json` beside six other sections. Three ways for the editor to write it:

- **A. The whole file through the writer (chosen).** `GameRules` joins `SerializedCatalogs`; the file is relaid once
  (about 40 lines become about 110). Safe because of the stamps (decision 559): the file is written only when the
  draft's rules differ from the bytes stamped at load, so a session that never touches the table never rewrites it;
  and when one does, a hand edit made on disk since the load makes Save all refuse with the file named
  (`CatalogFileStamps.IsEditedOnDisk`), so no hand edit is lost silently. The content rule already asks every content
  file for the writer's layout. Every later rules page costs only its boxes. Cost: the relay's layout diff; the round
  trip must hold for every section (risk 1). One slice.
- **B. The table in its own file, `experience.json`.** `rules.json` keeps its hand layout. Cost: a content split
  (loader, the test world, `GameRules` from two files), and the same question for the next rules page. About one and a
  half slices.
- **C. A patch of the `experience` node, the rest of the text kept.** No layout change. Cost: a second write path beside
  the writer's, outside the stamps' compare-the-bytes rule. Refused: two ways to write content is what ADR 0011 avoids.

## 5. Skills learned at a level (ADR 0062)

- **Shape (LV1):** `ClassDefinition.Skills`, a list of `ClassSkill(SkillId) { Level = 1 }`; in the file
  `"skills": [ { "skillId": "fire_blast" }, { "skillId": "meteor", "level": 10 } ]`, `level` left out at 1.
  `ClassDefinition.SkillIds` becomes computed (`[.. Skills.Select(s => s.SkillId)]`, left out of the file by
  `ComputedPropertyFilter`), so Data's checks and row 7's readers keep reading every id in order. The relay through the
  writer converts the repository's file; every level is 1, so nothing changes in play.
- **Rule (LV2):** `PartyMember` resolves the class's entries at recruit and knows those at or below its level
  (`Skills`), refreshed on a level up and at `RestoreProgress`; `SkillsLearnedAt(level)` names those of one level.
  `BattleRewards` fills `ExperienceGain.LearnedSkills` (an init property, so no constructor site changes) from the
  levels the steps reached. A battle builds its combatants from `Skills` when it starts, so a skill learned at its end
  is usable from the next battle.
- **Data (LV1):** a level below 1 is the class validator's message; a level past the table's max is
  `ClassSkillLevelCheck`'s, run by the loader after the rules and by the writer and both pages' problem lines.
- **Options set aside:** a `skillLevels` map beside `skillIds` (two structures to keep in step, a level for an unlisted
  skill to refuse); a separate level-up table per class (a third file the Classes page would edit beside the class).
- **Saves:** unchanged (decision 966).

## 6. File layouts, and what is left for later

`classes.json`, a class with one resource (decision 944): record fields in declaration order, a resource's absent rules
and zero gains left out, `skills` from LV1.

```json
{
  "id": "mage",
  "name": "Mage",
  "description": "...",
  "base": { "maxHp": 150, "speed": 10, "primaries": { ... }, "accuracy": 0 },
  "growth": { ... },
  "skills": [
    { "skillId": "fire_blast" },
    { "skillId": "meteor", "level": 10 }
  ],
  "resources": [
    { "id": "mana", "name": "Mana", "max": 10, "startsAt": 3, "gains": { "perTurn": 2 }, "color": "#5B8CFF",
      "willScaling": { "willPerPoint": 10, "effect": "raiseStart" } }
  ]
}
```

(Shown compact here; the writer puts one field per line.) A resource without gains writes `"gains": {}`; a speed
resource writes no `max` or `startsAt`. `party-members.json` keeps its fields' order, sorted by id. `new-game.json` is
already as the writer lays it out (`ContentWriterRepositoryTests.NewGame_RepositoryFile_WritesBackUnchanged`).
`rules.json` keeps `GameRules`' declaration order (party, experience, exploration, stats, loot, crafting, trade).

**Later:** a member's starting gear (decision 960: a `startingEquipment` list worn at recruit, about three slices, no
save change); per-class experience curves; the rest of `rules.json` as a page.

## 7. Risks

1. **Rarity-keyed dictionaries in `rules.json`** (`loot.rarities`, `crafting.salvage`, `crafting.upgradeCosts`):
   System.Text.Json may write an enum key as `Common`. XP1's relay test finds out first; the contingency (not in the
   estimate) is a `Rarity` property-name converter, never `DictionaryKeyPolicy`, which would rename every string key.
2. **Doubles:** `exploration.walkSpeed` 3.0 writes as `3`; the value is the same, the relay shows it once.
3. **A lowered max level lowers saved members** at the next load (`SaveGameRepair`): the Experience page says so while
   the max is below the saved one (XP2); a class skill past the new max is LV1's refusal on its line.
4. **Four relay commits** with layout-only diffs (CL1, LV1, PM1, XP1): each alone, "relay" in its message, its test
   comparing the catalog loaded before and after.
5. **The skill shape change touches row 7's code** (`SkillIds` readers): kept working by the computed `SkillIds`; the
   writers of `SkillIds` in tests (`ClassBuilder`, about ten sites, and row 7's fixtures) move to `Skills` in LV1, which
   greps them first. Row 7's balance table shows a class's skills at every level, learned or not; a "learned at" mark
   there is a follow-up for the backlog.
6. **Parallel rows touch the same Data files** (`ContentWriter.Catalogs`, the `AbsentWhen*` filters,
   `ContentWriterRepositoryTests`' list): a few lines each; the roadmap's order puts this plan last.
7. **Separate sessions:** the Classes page names skills, a skill names its class's resources, the New game names
   members: each page shows another's change as Data's message on its line and in Save all, never as a crash.
8. **Per-keystroke work:** each typed field runs Data's check over the whole list (nine classes, nine members) and the
   Levels tab rebuilds at most 99 rows from the fill (more only if typed by hand); the list virtualizes, so both stay
   well under a frame.
9. **Determinism and saves:** nothing random, no wall clock, no save format change (decisions 943, 966).
