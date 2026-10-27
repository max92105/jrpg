# M2b. Defense leaves

Part of [the stats and gear slots plan](../README.md). ADR 0027 (resistances replace defense), decisions 299, 331 (Guarded raises all eight). Design: "Replacement mapping", "Tunables" (statuses, enemies). After M2a.

- **M2b. Defense leaves; Guarded raises resistances; enemies deal damage, the goblin's retuned** (Core + Data + content, about 230 lines)
  - **Goal:** defense is gone from classes, items, enemies and the battle; a hit is its scaled power through resistance and Exposed. Guarded raises every resistance by its power in points (still within the cap). An enemy's `attack` becomes `damage`. After this slice nothing in the game says attack or defense.
  - **Changes:**
    - `Stats/CombatStats`: loses `Defense`; `Attack` is renamed `AttackDamage`. `PartyMember.BaseStats` and `TotalStats` go: `Battle/MemberBattleStats` (D1) builds the whole line from the sheet and the worn weapon.
    - `Party/ClassStats`: loses `Defense` (`ClassGrowth`, validator, `classes.json`).
    - `Items/EquipmentStats`: loses `DefenseBonus` (validator, `items.json`; the armors keep their P4 bonuses).
    - `Battle/EnemyDefinition`: `Attack` → `Damage`, `Defense` goes (validator, `enemies.json`, `EnemyDefinitionBuilder.WithStats(maxHp, damage)`; `WithDefense` and its users go). **The goblin is retuned** (review edit): its `damage` becomes 9, so the fighter takes about what it takes today (attack 16 − defense 8 = 8; now 9 less the fighter's 3.5 % physical resistance, rounded down once, is 8). The training dummy's `attack` 20 becomes `damage` 13 by the same rule (20 − 8 = 12 today; 13 less 3.5 % is 12). The Done note gives each class's goblin hit before and after.
    - `Effects/Mitigation` and `DamageFormula`: `Defense` goes: `max(1, power × (1000 − resistance) × (1000 + exposed) ÷ 1000²)`, in a `long`, rounded down once; the class summary's "placeholder ported from the demo" becomes the ADR 0027 formula with its reasoning.
    - `Statuses/StatusModifiers.Apply`: Guarded raises `Resistances` by its power (`Resistances.Raised`); `StatusKind.Guarded`'s summary says so. `content/skills.json`: aura 20, wide aura 12 (placeholders).
    - `Party/FieldItemUse`: `Mitigation.None`.
    - Text: `stat_attack`, `stat_defense`, `slot_armor` stay only if still used (grep; the Equipment page's armor label goes in G1a-1).
  - **Tests:**
    - `StatusModifiersTests: Apply_Guarded_RaisesEveryResistance` (replaces the defense one)
    - `StatusModifiersTests: Apply_GuardedPastCap_StopsAtCap`
    - `BattleStatusTests: Execute_AttackOnGuardedAlly_DealsLess` (rewritten from defense to resistance; in a new `BattleGuardedTests` if `BattleStatusTests` passes 280)
    - `CombatantTests: FromPartyMember_SwordWorn_AttackDamageIsSword`
    - `EnemyCatalogValidatorTests: Validate_NegativeDamage_NamesTheField`
  - **Call sites:** `new ClassStats(` 18 (the `Defense` argument goes); `CombatStats(` constructions 31 in 10 files (most gone with E1's previews: count the rest); `DefenseBonus` 3 (builder); `DamageFormula.Calculate` 7; the tests M2a listed with `WithDefense`.
  - **Size:** about 60 Core changed or removed, 25 Data and content, 5 tests (about 65), call sites about 80: about 230.
  - **Verify by hand:** a fight against goblins: a goblin hits the fighter for about 8, as before; hits land their scaled power through resistances; the paladin's aura lowers what allies take. The goblin trio simulation: every class wins every seed (if one doesn't, say so and propose a number; balance is the developer's); record the table.
- **Done, with deviations:**
