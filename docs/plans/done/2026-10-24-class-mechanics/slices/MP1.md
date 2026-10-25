# MP1. Skills and effects stop using MP

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Replacement mapping", "Key design decisions" 9. Decision 197; ADR 0022.

- **MP1. Skills and effects stop using MP** (Core + Data + content + Game, about 200 lines)
  - **Goal:**
    - No skill costs MP and no effect restores it. Skills cost only class resources and requirements, so the cleric's heals are gated by faith alone.
    - Members and screens still have MP (MP2, MP3b), unused.
  - **Changes:**
    - Core:
      - `Skills/SkillDefinition` loses `MpCost`;
      - `Effects/EffectKind` loses `RestoreMp`, and `EffectResolver` its case;
      - `Battle/SkillPayment` loses the MP check and spend;
      - `BattleState.LackingResourceFor` returns non-null whenever `CanAfford` is false, and its summary says so.
    - Data:
      - `SkillCatalogValidator` loses the `mpCost` check;
      - the content reader rejects an `mpCost` field, since the strict options fail on unknown fields. Check that this is how strictness works there, and note it.
    - Content: `skills.json` drops every `mpCost`.
    - Game:
      - `SkillCostText` loses the MP branch;
      - `FloatingNumbersView` loses the RestoreMp color;
      - `MessageIds.BattleSkillCost` and `BattleNotEnoughMp` and their `ui.json` lines go (the MessageIds test guards it);
      - `CommandMenuTests`' MP case moves to a resource case.
    - Tests:
      - `BattleSkillAndItemTests`' MP cases are deleted;
      - `EffectResolverTests`' RestoreMp cases are deleted;
      - `SkillCatalogValidatorTests`' `mpCost` case is deleted;
      - `skill-without-description.json` test content loses `mpCost`.
  - **Tests:**
    - `SkillPaymentTests`: `CanPay_ClericWithNoMp_HealStillUsable` (fails before: today it needs MP)
    - `BattleStateResourceTests`: `LackingResourceFor_Unaffordable_NeverNull`
  - **Verify by hand:** as Zanyr, heals show "Faith 1" and the like, never MP. As Omer, spells show mana only.
- **Done, with deviations:** Tests 1838 before, 1834 after: 7 MP tests deleted, 3 added. Beyond the plan:
  - Strictness checked: `ContentJsonOptions.Strict` uses `UnmappedMemberHandling.Disallow`, so a stray `mpCost` now fails the load naming the file.
  - The item message `item_used_mp` and `UiTheme.FloatingMp` went with `RestoreMp`.
  - `SkillCostText.CostOf` returns an empty line for a free skill. `CannotAffordText` now takes a non-null resource, and the panel skips it for rows that can be paid.
  - `CommandMenuTests`' MP case is merged into the Meteor test (9 mana against 3), which also checks the row is greyed and no target opens; the Omer setup is a shared `OmersSkillList` helper.
  - `Vitals.TrySpendMp` and `RestoreMp` have no caller left outside `VitalsTests` and one battle test; MP3b removes them with the rest of `Vitals`' MP.
