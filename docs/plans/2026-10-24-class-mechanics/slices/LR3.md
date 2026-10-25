# LR3. Clarity and lasting faith

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Content format" (clarity), "Tunables and placeholders" (mage, cleric rows). Decisions 189, 198, 205; question 219.

- **LR3. The mage's Clarity lowers starting mana; the cleric's faith lasts** (Core + Data + content, about 200 lines)
  - **Goal:**
    - The mage gets Clarity, a lasting resource (max 3, starts at 3) that drops by 1 for each battle fought.
      - Each missing point lowers the mana the mage starts a battle with by 1, down to 0.
      - Three battles in, Omer starts at 0 mana until he rests.
    - The cleric's faith becomes lasting: what she loses stays lost until a rest.
    - M3's rows show Clarity like any resource.
    - A rest comes in RS1-RS3; until then, a new game resets everything.
  - **New types:** Core `Resources/StrainRule(string LowersStartOf, int PerMissingPoint)` (public sealed record): how a lasting resource's missing points lower another resource's start.
  - **Changes:**
    - Core: `ResourceDefinition.Strain` (`StrainRule?`). `Battle/ResourceStarts` lowers the strained resource's start by (source max − source amount) × per point, never below 0.
    - Data: `ResourceDefinitionValidator`: a strain's source is lasting; `lowersStartOf` names another resource of the class; per point ≥ 1.
    - Content (`classes.json`):
      - the mage gets `clarity` (`isLasting`, `perBattleFought` −1, strain on `mana`);
      - the cleric's `faith` gets `isLasting`.
  - **Tests:**
    - `CombatantTests`:
      - `FromPartyMember_TwoClarityMissing_ManaStartsTwoLower`
      - `FromPartyMember_StrainBeyondStart_StartsAtZero`
    - `GameSessionBattleTests`:
      - `FinishBattle_MageFought_ClarityDropsByOne`
      - `FinishBattle_AllyFellInBattle_FaithStaysLowered`
    - `ClassCatalogValidatorTests`: `Validate_StrainFromNonLasting_Throws`
  - **Verify by hand:**
    - As Omer, three fights: mana starts at 3, 2, 1, then 0, and CLARITY reads 2, 1, 0.
    - Save, quit, continue: the same.
- **Done, with deviations:**
  - The validator tests sit in `ResourceDefinitionValidatorTests` (moved there in DR2), with three more than planned: a strain naming an unknown resource, a per-point of 0, and the LR1 review's follow-up, which now refuses a lasting resource that takes its max from speed.
  - The faith test is `FinishBattle_FaithLostInBattle_StaysLowered`, with faith lost per hit taken: the session tests have one member, so no ally can fall. The ally-fall trigger itself is already tested since CL1.
  - After review: a strain may not lower a lasting resource, whose lowered start would carry on and drop again each battle (one more validator test).
