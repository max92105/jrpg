# EF1. Effect strip records: `effect-strips.json` (pace, hit frame, swing), loaded and checked

Core + Data, about 210 lines (about 120 of them tests). Needs the animated-monsters plan's MB8 (its `EffectTiming`) and
ADR 0048. Decisions 654, 657; design "Core changes", "Content format"; tunable "Unlisted strip pace".

## Goal
`content/effect-strips.json` can give an effect strip, by its file name, its frame time, the frame its hit lands on and
the swing strip an attack plays before it. A strip it doesn't list plays at 80 ms a frame with its hit on frame 0 and no
swing, as every strip does today. The file is loaded and checked with the other catalogs (a field-level check here; the
art check is EF2's). This slice ships the file empty (`[]`); EF7 fills it. Nothing plays differently yet (EF3 reads it).

## New types
- `Core/Battle/EffectStripDefinition` (public sealed record): one effect strip's pace: its frame time, the frame its hit
  lands on, the swing it plays first. `string File`, `int FrameMilliseconds`, `int HitFrame` (init, 0 by default),
  `string? SwingFile` (init, null by default); `TimeSpan FrameTime`; `TimeSpan HitLead` (`HitFrame` x `FrameTime`);
  `static TimeSpan UnlistedFrameTime` (80 ms, decision 57's value, moved from MB8's `Battle/EffectTiming.FrameTime`);
  `static EffectStripDefinition Of(string file, IReadOnlyDictionary<string, EffectStripDefinition> listed)` (the listed
  record, else the unlisted pace with hit frame 0 and no swing). Its summary names ADR 0048 and where a projectile would
  go later (decision 665).
- `Data/Battle/EffectStripCatalogValidator` (internal static): checks `effect-strips.json`: each `file` not blank and
  listed once (ordinal, as file names are matched), `frameMilliseconds` at least 1, `hitFrame` at least 0, a present
  `swingFile` not blank; a message names the file and the strip's position or name.

## Changes
- `Core/Battle/EffectTiming` (MB8) is deleted; its readers (`Game/Battle/EffectStrip`, the editor's effect import in
  `Editor/Catalogs/ArtImageImport`) read `EffectStripDefinition.UnlistedFrameTime`. The import's status line becomes
  "effects play at 80 ms a frame unless effect-strips.json lists them" and its test re-pins that wording.
- `Core/Content/ContentCatalog.EffectStrips` (`IReadOnlyDictionary<string, EffectStripDefinition>`, by file,
  non-required, empty by default, as `FieldSprites`).
- `Data/Content/ContentCatalogLoader`: `EffectStripsFile` = `"effect-strips.json"` (internal, EE1 writes it), read
  through `ReadValidated` with the validator, frozen by file (ordinal).
- `content/effect-strips.json`: `[]` (CRLF, trailing newline).
- Grep `ContentCatalogLoader.Load(` in `tests/`: a test that builds a content root from scratch (not a copy of the
  repository's) gets the empty file; budget about 3 such sites.
- `docs/map.md`: Core `Battle` names `EffectStripDefinition`; Data's validator list names `effect-strips.json`.

## Tests
- `EffectStripDefinitionTests` (new, `tests/Jrpg.Core.Tests/Battle/`), records built in the test:
  `Of_Unlisted_PlaysAtTheUnlistedPaceWithNoSwing`; `Of_Listed_IsItsRecord` (a 17 ms record with a swing, not the
  default); `HitLead_HitFrameTwo_IsTwoFrameTimes` (80 ms: 160 ms); `FrameTime_IsTheMilliseconds`.
- `EffectStripCatalogValidatorTests` (new, `tests/Jrpg.Data.Tests/Battle/`): an `InvalidStrips` theory with rows for a
  blank file, a file listed twice, `frameMilliseconds` 0, `hitFrame` -1, a blank `swingFile` (each message checked for
  the file's name or position); `Validate_LowestPaceAndHitFrameZero_Passes` (1 ms, hit frame 0: the accepted bounds);
  `Validate_NoSwing_Passes`.
- `ContentCatalogLoaderEffectStripTests` (new): `Load_ListedStrip_ReadsItsPaceAndSwing` (a copied content root with a
  one-record file: 80 ms, hit frame 2, a swing); `Load_NullEntry_NamesTheFile`;
  `Load_RepositoryContent_HasTheEffectStripsCatalog` (loads; no totals asserted).
- MB8's `ArtImageImportTests: Import_GifIntoEffects_ReportsItsLength` re-pinned to the new status wording only.

## Verify by hand
1. Run the game: it starts as before (the empty catalog changes nothing).
2. Put `[{ "file": "AttackEnemy.png", "frameMilliseconds": 0 }]` in `content/effect-strips.json` and run: the start
   stops naming the file and the frame time. Revert (`git checkout content/effect-strips.json`).
