# EE1. `effect-strips.json` saved from the editor

Data + Editor, about 200 lines (about 110 of them tests). Needs EF1. Decision 659 (Q11); ADR 0045; design "Content
format". Modeled on the drop table editor plan's DT1.

## Goal
The editor can write `effect-strips.json`: records sorted by file, `hitFrame` left out at 0 and `swingFile` while null,
so the hand-written `[]` and the developer's records read as the writer writes them. Data gives the editor's live
problem line its message for a whole list (`CatalogProblems.OfEffectStrips`), and the draft takes a new list as one
undo step, compared by content. No window yet (EE2-EE4).

## Changes
- `Data/Content/ContentWriter.Catalogs.cs`: `effect-strips.json` joins the written catalogs, serialized sorted by file
  (ordinal); `hitFrame` through `AbsentWhenZeroFilter`, `swingFile` through `AbsentWhenNullFilter` (the filters'
  per-type registration, as the drop tables' quantity in DT1).
- `Data/Content/CatalogProblems.OfEffectStrips(ContentCatalog content, IReadOnlyList<EffectStripDefinition> strips)`:
  the loader's own message for the list (EF1's validator), null when it passes.
- `Editor/Catalogs/CatalogDraft.Database.cs`: `SetEffectStrips(IReadOnlyList<EffectStripDefinition> strips)`: one undo
  step, none when the records are the same by content (`HasSameRecords`, records keyed by file).
- `docs/map.md`: the Data `Content` row's `CatalogProblems` list and the Editor `Catalogs` row name the effect strips.

## Tests
- `ContentWriterEffectStripTests` (new, `tests/Jrpg.Data.Tests/Content/`), records built in the test:
  `Write_StripWithHitFrameZero_LeavesItOut`; `Write_StripWithoutSwing_LeavesItOut`; `Write_TwoStrips_SortsThemByFile`
  (set up in the opposite order); `Write_StripsWithEveryField_LoadBackTheSame`.
- `ContentWriterRepositoryTests`: passes with `effect-strips.json` among the files it compares byte for byte (the
  repository's `[]`).
- `CatalogProblemsTests`: `OfEffectStrips_FileListedTwice_NamesIt`; `OfEffectStrips_ValidList_IsNone`.
- `CatalogDraftEffectStripTests` (new, `tests/Jrpg.Editor.Tests/Catalogs/`): `SetEffectStrips_Changed_AddsOneStep`;
  `SetEffectStrips_SameByContent_AddsNoStep`; `SetEffectStrips_Undo_RestoresTheOldRecords` (its own test).

## Verify by hand
None: no window yet. EE4's hand check saves through this.
