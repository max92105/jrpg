# SK6. A skill's effect: tier, kind, targeting, hits

Editor, about 240 lines (about 120 of them tests; the section's rules and its 40 XAML lines can't split without a
slice that shows nothing). Decisions 980 (row 6's 792, 815). Design: design.md, sections 2.4
and 2.6 ("Picks resolve at apply"). Builds on SK5.

## Goal
The Skills page's Effect section: a Tier box (none, or a record: "fire_1: damage, fire, x2"); choosing one sets Kind
and Damage type from the record and greys them and Power (written 0); with none, Kind (every `EffectKind` but `rest`),
Damage type and Power are typed; Targeting (every `Targeting`), Range (enabled for `alliesInRange` only), Hits. Data's
rules (a type only on damage, a power beside a tier, hits above 1 only on damage, a revive's targeting) show as its
message. The grants keep their file values until SK7.

## New types
- `Editor/Effects/TierChoice` (internal sealed record; in `Effects/` so row 9's IU12 reuses it): one Tier box entry: `Id` (empty for none), `Line` ("fire_1:
  damage, fire, x2"; "heal_2: restoreHp, x4"; "(none: a flat power)"), `Tier` (`TieredEffect?`); `static
  IReadOnlyList<TierChoice> AllIn(ContentCatalog content)`: none first, then the records by id.
- `Editor/Skills/SkillEffectSection` (internal sealed partial, `ObservableObject`): a skill's effect as typed.
  - `TierId` (empty for none), `Kind`, `DamageType` (`DamageType?`; null for none), `PowerText`, `Targeting`,
    `RangeText`, `HitsText`; `Kinds` (`Enum.GetValues<EffectKind>()` less `Rest`), `Targetings`, `DamageTypes` (null
    first, then every type).
  - `IsTiered`, `CanTypeKind` (no tier), `CanTypePower` and `CanTypeDamageType` (no tier, and the kind reads it), `CanTypeRange`
    (`alliesInRange`): through a private `FieldsFor(EffectKind kind)` switch (damage: power, type, hits; restoreHp and
    revive: power; grantStatus and readyExtraTurn: none; its default arm throws), so the setters stay under CA1502.
  - Choosing a tier sets `Kind` and `DamageType` from the record and `PowerText` to "0"; choosing none keeps them.
  - `Problem`: a box that isn't a whole number, by name; `ToEffect(IReadOnlyList<TierChoice> tiers)`: the `Effect`
    with its tier resolved from the choices (read from the draft when the page opened; one session at a time, so
    nothing changes tiers while it is open), a greyed power written 0, a greyed type null, range 0 unless `alliesInRange`, statuses empty.
  - `event EventHandler? Changed` for typed fields only.

## Changes
- `Editor/Skills/SkillChoices`: `IReadOnlyList<TierChoice> Tiers`.
- `Editor/Skills/SkillViewModel`: `Effect` (a `SkillEffectSection` from the record); `Problem` reads it after the
  costs; `ToSkill` sets `Effect = Effect.ToEffect(choices.Tiers) with { Statuses = the loaded effect's }` (SK7
  replaces the last part); `Changed` relays it.
- `Editor/Skills/SkillViewModel.New` (SK3): starts its section on the first damage tier.
- `Editor/Skills/SkillCatalogPage.xaml`: an "Effect" section band after Costs: Tier (combo of `Choices.Tiers` by
  `SelectedValue` = `TierId`, `DisplayMemberPath="Line"`), Kind, Damage type, Power, Targeting, Range, Hits, each
  enabled by its `CanType...`; tooltips: Tier "A named damage or heal: the user's Power times its per-Power number
  (Database > Effects...)", Power "Used without a tier: HP before the user's Power raises it". About +40 lines.

## Tests
- `Editor.Tests/Skills/SkillEffectSectionTests` (new; `SkillFixtures`):
  - `From_TieredSkill_ShowsTheTierAndGreysKindAndPower`.
  - `ChooseTier_HealRecord_SetsKindRestoreHpAndNoType`.
  - `ChooseTier_None_KeepsKindAndTypeTypeable`.
  - `CanTypeRange_AlliesInRange_True`, `CanTypeRange_SingleEnemy_False`.
  - `CanTypePower_GrantStatus_False`.
  - `ToEffect_Tiered_WritesPowerZeroAndTheRecord`.
  - `ToEffect_FlatRevive_KeepsTheTypedPowerAndNoType`.
  - `ToEffect_RangeTypedOffAlliesInRange_WritesZero`.
  - `Kinds_EveryKindButRest_Offered` (a theory over `EffectKind`).
  - `Problem_HitsNotANumber_NamesHits`.
- `Editor.Tests/Skills/SkillViewModelTests`: `ToSkill_EffectEdited_KeepsTheLoadedGrants`.
- `Editor.Tests/Effects/TierChoiceTests` (new): `AllIn_NoneFirstThenById`, `Line_DamageRecord_ReadsKindTypeAndPerPower`.

## Verify by hand
1. Database > Skills..., `fire_blast`: Tier `fire_1`, Kind and Damage type greyed at damage and fire, Power 0 greyed.
2. Tier `fire_2`, OK, Save all: `git diff content/skills.json` shows `"tier": "fire_2"` only. Run the game, fight with
   the mage: Fire Blast hits harder. Revert with Ctrl+Z and Save all.
3. Tier none, Kind damage, Damage type ice, Power 12: OK lights (a flat damage); Cancel.
4. `aura_of_protection`: Range enabled with its number; Targeting single ally: Range greys.
5. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` or binding-error line.
- **Note from SK3:** with no damage tier, `SkillViewModel.New` starts a flat damage of power 1 typed as Core's first
  damage type (Data needs one); Data refuses a skill's damage without a tier (decision 704), so check step 3's "OK
  lights".
