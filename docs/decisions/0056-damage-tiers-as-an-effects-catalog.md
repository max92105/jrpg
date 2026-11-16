# 0056. Damage tiers as an effects catalog

Status: accepted
Date: 2026-11-15

## Context
A skill's damage or heal is a flat `power` today, raised by its user's Power percent (`CombatStats.DamagePermille`,
decision 696). The developer's "Damage Test" sheet asks for tiers instead: damage is Power times a tier (Low 2,
Medium 4, High 9, Massive 22, Extreme 58 per Power), times 1 plus the sum of the modifiers (gear, statuses), and he
named the records himself: "that is why there is fire_1 fire_2 or poison_1 etc." (decisions 703, 704). Constraints:
numbers stay content; the battle never reads a catalog at run time; `Effect.Kind` has many readers; skills, items and
enemies already load straight into Core records (ADR 0002); the products must stay within a `long`.

## Options
### A. Tiers in `rules.json`, named by the skill's effect
`"tier": "low"` against a five-entry `damageTiers` table.
- Pros: one number per tier; no catalog and no editor window.
- Cons: no `fire_1` record, against the developer's two statements; no per-record tuning (a fire High unlike an ice
  High).

### B. An `effects.json` catalog of named records (chosen)
`{ "id": "fire_1", "kind": "damage", "perPower": 2, "damageType": "fire" }`; a skill's effect writes
`"tier": "fire_1"` beside its own kind and damage type.
- Pros: the developer's vocabulary; one record per named effect, tuned on its own; everything naming `fire_1` reads the
  same numbers; items could name records later.
- Cons: records exist only as content needs them (11 at first), not one per type and tier; the skill keeps writing its
  kind and damage type, so Data must refuse a mismatch with the record; one more Database window later (row 7); and
  the references resolved at load must be rebuilt when the editor changes a record.

### C. Keep today's flat power raised by Power's percent
- Cons: decision 704 asked for tiers.

## Decision
Option B (decisions 782-794, 817, 818). `content/effects.json` holds named records of kind `damage` (with a damage
type) or `restoreHp`; ids are `<type>_<n>` and `heal_<n>`, the suffix naming the sheet's tier. A skill's effect names
one as `tier`, resolved to the record at load by one converter (`Data/Content/CatalogReferenceConverter<T>`, decision
789), so the battle reads records, never the catalog; the same converter later resolves a status grant's record. A
tiered amount is the user's Power times the record's `perPower` times (1 + the sum of the damage modifiers), one `long`
product rounded down once and held at `DamageFormula.MaxPower` (`DamageFormula.TieredAmount`); crit, resistance,
Exposed's share and block then apply as before, and a dominant force multiplies after the sum (decision 817). Power's
damage percent no longer raises a tiered effect (it would count Power twice); it still raises the basic attack and any
flat effect. Enemies get a content `power` for their tiered skills; items keep a flat power (decision 785).

## Consequences
- Every skill in `skills.json` names its tiered record; a tiered effect keeps `"power": 0` in the file, and Data
  refuses any other power beside a tier, or a kind or damage type that differs from the record's.
- Data bounds a record's `perPower` at 1000 and an enemy's power at 10 000; an enemy whose tactics or phases name a
  tiered skill needs a power of at least 1.
- The modifiers' sum starts with one source (Weakened, decision 806); gear's "+X % damage" joins it with the items plan.
- Row 7's editors get an Effects window and must rebuild the skills naming a record they edit, and register the
  converter for writing (design.md, section 6 of the skills and statuses plan).
- No save format change: neither effects nor skills are saved.
