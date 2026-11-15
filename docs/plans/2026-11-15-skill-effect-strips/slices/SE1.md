# SE1. A skill or item names its strip, checked at load and at startup

Core and Data, about 220 lines (code about 55, tests about 165). Needs the ADR 0048 amendment. Decisions 706, 707,
709, 710; design.md "Core changes", "Structure and folder dependencies".

## Goal
`skills.json` and `items.json` may give a skill or an item an `effectFile`. The loader reads it, refuses a blank one
and one on an effect that doesn't damage, and the game's startup art check refuses a named strip that is missing from
`effects/` or isn't whole square frames. Nothing plays it yet (SE2); no content names one (SE3).

## New types
- `Data/Effects/OwnEffectStripValidator` (internal static): checks the strip a skill or item names for itself
  (decisions 706, 707). `public static string? FindProblem(string? effectFile, Effect? effect)`:
  - null: no problem;
  - blank or whitespace: `"effectFile must name a strip in effects/ when present."`;
  - `effect` null or not `EffectKind.Damage`: `"effectFile is only for a damaging effect (decision 707)."`.

## Changes
- `Core/Skills/SkillDefinition`: `public string? EffectFile { get; init; }`, doc: the strip in the effects folder its
  action plays instead of its user's attack strip, paced by its `effect-strips.json` record (decision 678); null plays
  the user's.
- `Core/Items/ItemDefinition`: the same property and doc, "an item used in battle".
- `Data/Skills/SkillCatalogValidator.Validate`: its lambda becomes `skill => FindProblem(skill) ??
  OwnEffectStripValidator.FindProblem(skill.EffectFile, skill.Effect)`; `FindProblem` is untouched.
- `Data/Items/ItemCatalogValidator.Validate`: the same shape, `item => FindProblem(item) ??
  OwnEffectStripValidator.FindProblem(item.EffectFile, item.Effect)`. `FindProblem` is untouched (it sits at CA1502's
  limit); a material, equipment or effect-less item still reaches the strip check, since `FindProblem` returns null
  for a valid one.
- `Data/Art/ArtReferenceCheck.Battle.cs` `AddBattleArtProblems`: the `attackEffects` list also takes
  `catalog.Skills.Values` with an `EffectFile` (named `skill '<id>'`) and `catalog.Items.Values` with one (named
  `item '<id>'`), each ordered by id, ordinal, so the exact-name check and `AddPartialFrameStrips` cover them; the class
  summary says "attack effects, skills' and items' own strips (decision 709)". Rename the local to `impactStrips`, since
  it is no longer only attacks.
- `docs/map.md`: Skills/Stats row, `SkillDefinition` (with `EffectFile`, its own strip, decision 678); Items row,
  `ItemDefinition` the same; Data Art row, "attack effects and skills' and items' own strips"; the Data validators row
  names `Effects/OwnEffectStripValidator`.

## Tests
- `SkillCatalogValidatorTests` (98 lines): two rows in `InvalidSkills` and one fact, over `_heal` and a damaging copy
  built in the test (`_heal with { Effect = new Effect(EffectKind.Damage, Power: 10, Targeting.SingleEnemy) }`):
  - row `damaging with { EffectFile = " " }` -> `"effectFile must name a strip in effects/ when present."`;
  - row `_heal with { EffectFile = "Burst.png" }` -> `"effectFile is only for a damaging effect (decision 707)."`;
  - `SkillCatalogValidatorTests: Validate_DamagingSkillNamingAStrip_DoesNotThrow`.
- `ItemCatalogValidatorEffectFileTests` (new sibling: `ItemCatalogValidatorTests` is at 248 lines; Glob found no file
  of that name), items built in the test (a damaging consumable, a healing one, a weapon, a material):
  - `Validate_DamagingItemNamingAStrip_DoesNotThrow`
  - `Validate_BlankEffectFile_ThrowsNamingTheField`
  - `Validate_HealingItemNamingAStrip_ThrowsNamingTheRule`
  - `Validate_WeaponNamingAStrip_ThrowsNamingTheRule` (no effect at all: the rule's other side)
  - `Validate_MaterialNamingAStrip_ThrowsNamingTheRule` (a valid material reaches the strip check through `Validate`'s chain)
- `ArtReferenceCheckEffectTests` (96 lines, about +45): the catalog `with` a `Skills` or `Items` dictionary holding one
  record built in the test (the test world's `fire_blast` or `bomb`, picked by id, `with { EffectFile = Strip }`):
  - `Check_SkillStripMissing_NamesTheSkill` (`effects/Burst.png: not found ... Named by skill 'fire_blast'.`)
  - `Check_ItemStripMissing_NamesTheItem`
  - `Check_SkillStripNotSquareFrames_NamesIt` (a 30 x 8 strip written by `WriteStrip`)
  - `Check_SkillStripPresent_Passes` (the filter's other side: a 32 x 8 strip, no line names it)
- `ContentCatalogLoaderEffectFileTests` (new; Glob found none), on a `CopiedContentRoot` of the test world, the field
  inserted after the record's id line (ids never change, `.claude/rules/content.md`), as
`ContentCatalogLoaderSkillSoundTests` edits a copied root:
  - `Load_SkillNamingAStrip_ReadsIt` (`"id": "fire_blast",` gains `"effectFile": "Burst.png",`; asserts
    `catalog.Skills["fire_blast"].EffectFile` is `"Burst.png"`)
  - `Load_ItemNamingAStrip_ReadsIt` (the same on `"id": "bomb",`)
- No Core test: the fields carry no behavior. No existing test breaks (init properties, null default).

## Verify by hand
1. In `content/skills.json`, add `"effectFile": "Missing.png",` after `fire_blast`'s `soundFile` line; run the game:
   the start stops naming `effects/Missing.png` and `skill 'fire_blast'`.
2. Change it to `"effectFile": "GoldenSlashImpact.png",` and move it onto `heal` instead: the start stops naming
   `skills.json`, `skill 'heal'` and "only for a damaging effect".
3. Revert: `git checkout content/skills.json`.

- **Done:** as planned (the two art-check summaries and the Items row of the map named after the review). The art check's strips come from a small `OwnStripsOf(catalog)` beside the attackers';
  `ContentCatalogLoaderEffectFileTests` also checks a skill without the field has none. The hand check (a missing strip
  stops the game's start) is covered by the art check's tests and left to the developer.
