# SC1. `statuses.json`: the status records, loaded and checked

Core + Data + content, about 270 lines of code and tests (about 150 of them tests), plus about 160 lines of
`statuses.json`. Needs ADR 0055. Design: design.md, sections 2.3 (the part kinds), 2.4, 3.3 (the records).

## Goal
`content/statuses.json` holds the ten records of design.md, section 3.3 (without TT's type, ST's timing, WK's and
SL's records), in the writer's layout. The game loads and checks it at startup and stops on a broken one, naming the
file and the record. The battle doesn't read it yet: it still runs on `StatusKind` until SW.

## New types
- `Core/Statuses/StatusPartKind` (public enum): what a part of a status does; the closed list (ADR 0055). Members with
  one-line summaries: `DamageEachTurn`, `HealEachTurn`, `SkipTurn`, `DrawAttacks`, `ResistanceChange`,
  `DamageTakenChange`.
- `Core/Statuses/StatusPart` (public sealed record): one rule a status applies while held, with its number.
  `StatusPart(StatusPartKind Kind, int Amount = 0)`; `Amount`'s doc: its meaning by kind (design.md, section 2.3).
- `Core/Statuses/StatusDefinition` (public sealed record): one status as content describes it (ADR 0055).
  - `required string Id`, `required string Name`, `required string Description` (may be empty), `bool IsHarmful`,
    `required IReadOnlyList<StatusPart> Parts`.
  - `Equals(StatusDefinition?)` and `GetHashCode` compare the parts in order (decision 795, as `DropTable`).
- `Data/Statuses/StatusCatalogValidator` (internal static): `statuses.json`'s rules.
  - `Validate(string fileName, IReadOnlyList<StatusDefinition> statuses)` over `ContentListValidator` (ids).
  - `FindProblem`: a name not empty; at least one part; no null part; at most one of `damageEachTurn` and
    `healEachTurn`; a kind at most once; each part's amount in its range (design.md, section 2.3), through a private
    `FindAmountProblem(StatusPart part, int index)` switch over the kinds (its default arm throws), so `FindProblem`
    stays under CA1502. Messages name the part: `"parts[0]: damageEachTurn's amount must be at least 1 (was 0)."`.

## Changes
- `Core/Content/ContentCatalog`: `IReadOnlyDictionary<string, StatusDefinition> Statuses { get; init; }`, empty by
  default as `EffectStrips` is: "the statuses an effect grants, by id (ADR 0055)".
- `Data/Content/ContentCatalogLoader`: `internal const string StatusesFile = "statuses.json"`; read and validated
  before the skills; `Statuses` set on the catalog.
- `Data/Content/AbsentWhenZeroFilter`: `(typeof(StatusPart), "amount")` ("ADR 0055: a skip or a taunt has no
  number.").
- `content/statuses.json`: ten records sorted by id, one field per line in nested objects, CRLF and a final newline, as
  `ContentFileWriter` writes them (`.claude/rules/content.md`). Descriptions: one short line each ("Loses HP each
  turn.", "Takes more from every hit.", ...); names from today's `ui.json` (Stun, Taunt, Guard, Exposed, Poison, Regen)
  and Burning.
- `docs/map.md`: the Statuses line names the records (the swap rewrites it).

## Tests
- `Core.Tests/Statuses/StatusDefinitionTests` (new; records built in the test):
  - `Equals_SamePartsInOrder_IsTrue` (two lists, two instances).
  - `Equals_PartsInAnotherOrder_IsFalse`.
- `Data.Tests/Statuses/StatusCatalogValidatorTests` (new; records built in the test): an `InvalidStatuses` theory (an
  empty name; no parts; a null part; `damageEachTurn` 0; `healEachTurn` 0; `skipTurn` with an amount; `drawAttacks`
  with an amount; `resistanceChange` 0; `damageTakenChange` -100; `damageTakenChange` 0; a damage and a heal part; a
  kind twice; an id twice), and at each accepted bound `Validate_DamageEachTurnOfOne_Accepted`,
  `Validate_DamageTakenChangeOfMinus99_Accepted`, `Validate_NegativeResistanceChange_Accepted`.
- `Data.Tests/Content/ContentCatalogLoaderStatusTests` (new):
  - `Load_RepositoryStatuses_StunSkipsATurn` (the `stun` record has a `skipTurn` part; no number pinned).
  - `Load_StatusesFileMissing_RefusesNamingIt` (`CopiedContentRoot` without the file).
  - `Load_StatusPartWithUnknownKind_RefusesNamingFileAndPath` (a part kind typed wrong).
- `Data.Tests/Content/ContentWriterRepositoryTests` (81 lines): `Statuses_RepositoryFile_WritesBackUnchanged` (the
  sorted records through `ContentFileWriter.Write` give the file's bytes, as `NewGame_RepositoryFile_WritesBackUnchanged`).

## Verify by hand
- Run the game: it starts and battles play as before.
- In `content/statuses.json`, set `poison_1`'s amount to 0 and run the game: the start stops on "statuses.json:
  poison_1: parts[0]: damageEachTurn's amount must be at least 1 (was 0)." (the exact prefix is `ContentListValidator`'s);
  revert.
