# Shops and the enemy database: design

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| New enemy | Editor `Battle/EnemyViewModel.New` | the validator's lowest legal numbers: max HP 1, damage 0, accuracy 75, everything else 0, blunt, no art | What Add starts from; Duplicate starts from the chosen enemy instead. |
| Quest giver prompt | Editor `Events/EventPresets` | "Talk" | As the talk preset's (placeholder). |

## Design overview

### What exists
- Core records: `Trade/ShopDefinition(Id, Name, ItemIds)`, `Battle/EnemyDefinition` (name, three art files, flip, max HP, damage, damage type, accuracy, dodge, crit, crowd control resistance, `Resistances`, experience, `DropTableId?`, `Gold?`, `IsBoss`, `Tactics`, `Phases`), `Battle/EnemyAction(SkillId?, Weight, When?)`, `Battle/TacticCondition(Kind, Percent)`.
- Data validators (internal): `Trade/ShopCatalogValidator`, `Battle/EnemyCatalogValidator` + `EnemyTacticsValidator`; `Battle/ResistancesConverter` reads percents by type and throws on write.
- From plan 1: CW1's `CatalogFileStamps` (a save writes only changed catalog files), SI1's `CatalogProblems` and `ContentWriter.ImportArtImage`, SI4's `Shell/StatusBand`.
- The editor's catalog pattern: `Catalogs/CatalogDraft` (`SetQuests`), `Quests/QuestCatalogViewModel`, `QuestViewModel`, `QuestEditingViewModel`, `QuestCatalogWindow`, `Shell/MainWindow.QuestEditing`, `Shell/CombinedHistory.RefusalOfQuestUndo`, `Catalogs/CatalogUsage.UsersOfQuest`, `Events/ListOrder`.
- Events: `Events/EventPresets` (talk, forge master, shopkeeper), `Events/StepChoices.ShopIds`, the dialogue step's typed `Speaker` and `PortraitFile`, `Events/QuestGateViewModel`.

### Core changes
None.

### Data changes
- `Content/CatalogProblems`: `OfShops`, `OfEnemies`.
- `Content/ContentWriter.CheckedCatalogWrites`: validates and writes `shops.json` and `enemies.json` (when changed, CW1).
- `Content/AbsentWhenFalseFilter`, `Content/AbsentWhenEmptyFilter` (EN1), on the shared `OptionalFieldFilter.SkipWhen`; rows in `AbsentWhenNullFilter` and `AbsentWhenZeroFilter` (decision 586).
- `Battle/ResistancesConverter.Write`: whole percents by type in `DamageType`'s order, a type at 0 left out.
- `Art/ArtFolders.ImagesIn(string artRoot, string folder)` (NP2): the PNG names of any art folder.

### Content format
`shops.json` and `enemies.json` are relaid once by the writer in their first writing slice (SH1, EN1): the loaded catalog before and after is compared, then the identical-bytes test covers the file. An enemy written by the editor:

```json
  {
    "id": "training_dummy",
    "name": "Training Dummy",
    "portraitFile": "Goblin.png",
    "battleSpriteFile": "Goblin.png",
    "attackEffectFile": "AttackEnemy.png",
    "isSpriteFlipped": false,
    "maxHp": 400,
    "damage": 13,
    "damageType": "blunt",
    "accuracy": 80,
    "dodge": 0,
    "critChance": 0,
    "crowdControlResistance": 50,
    "resistances": {
      "blunt": 25,
      "piercing": 25,
      "fire": -50
    },
    "experienceReward": 10
  }
```

### Structure and folder dependencies (Editor)
New editor folders `Trade/` (shops) and `Battle/` (enemies), each with its view models and window; no type is named after its folder.

| From | To | Why |
|---|---|---|
| Editor `Trade`, `Battle` | `Catalogs`, `Objects` (`ObjectIds`), `Events` (`ListOrder`) | the draft; the id rule; list moves |
| Editor `Shell` | `Trade`, `Battle` | the Database menu opens the windows |
| Data `Content` | Data `Trade`, `Battle` | `CatalogProblems` runs their validators (existing pattern) |

`Events` never reads `Trade` or `Battle`; the event dialog's shop box keeps reading `StepChoices.ShopIds` from the draft.

### Key decisions
1. **Data's message is the window's problem** (decision 580): the window checks what Data can't see, then shows `CatalogProblems`' message for the whole edited list ("enemies.json: enemy 'wolf': accuracy must be within 0..200 (was 250).").
2. **One step per window** (decision 591): `SetShops`, `SetEnemies`, each equal by content (lists compared element by element) and recording nothing when unchanged.
3. **Rows from enums**: resistances (`Enum.GetValues<DamageType>()`) and damage type, so a type added to Core appears in the window.
4. **What the window doesn't edit, it keeps**: `EnemyViewModel.ToEnemy` is the original record `with` the edited fields, so tactics and phases (hand-written until the Later forms) round-trip untouched.
5. **Pickers read folders through lookups** passed in (the shell's `ArtFolders.ImagesIn`), never files from a view model.

### Extension points
- Tactics and phases forms (Later: EN4, EN5, EN8), a "New encounter from these enemies" button (Later), items, drop tables, party members: the same four pieces per catalog (writer + `CatalogProblems`, the draft's setter, the rules, the window).
- A side-by-side balance table: a read-only grid over the draft's enemies (and, later, skills and classes).

### Replacement mapping

| Today | Replaced by |
|---|---|
| hand-editing `shops.json` and an enemy's numbers and art | the Shops and Enemies windows |
| `ResistancesConverter.Write` throwing | written in percents (EN1) |
| the Quests menu | the Database menu (SH3) |
| the dialogue step's typed portrait | an editable list of the portraits folder (NP2) |

## Risks
- **Relaying hand files**: the first write reformats `shops.json` and `enemies.json` (a large diff with no meaning change); the relay test proves the catalog is the same, the commit holds only that.
- **Filters hide fields**: a field left out at its default reads back the default; a test per filtered field both ways, and one asserting the relaid file holds none of them at their defaults.
- **Hand-written tactics beside the window**: after CW1, a hand edit to `enemies.json` while the editor is open is refused at the next save that changes enemies (not reverted); the developer reopens the editor after editing tactics by hand.
- **Coupling with plan 1**: CW1, SI1 and SI4 must be done first.
- **Bought art**: imported enemy art fails `ArtSourcesTests` until its line is added by hand; each hand check says so.
