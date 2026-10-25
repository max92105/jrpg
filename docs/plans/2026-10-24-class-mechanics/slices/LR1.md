# LR1. Lasting resources carry between battles

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Key design decisions" 8, "Structure". ADR 0024.

- **LR1. Lasting resources carry from battle to battle on the member** (Core, about 240 lines)
  - **Goal:**
    - A resource marked lasting starts each battle at the member's carried amount, not at its starting amount.
    - When a battle ends (any outcome), each active member's lasting amounts are written back, then their `perBattleFought` gain applies, within 0..max.
    - A new member starts with each lasting resource at its starting amount.
    - Not saved yet (LR2): a continued game starts lasting resources fresh. No content is lasting yet (LR3).
  - **New types:**
    - Core `Battle/LastingResourceCarry` (internal static): writes a finished battle's lasting amounts back to its party members and applies the per-battle-fought change: `CarryBack(BattleState battle)`.
  - **Changes:**
    - Core:
      - `Resources/ResourceDefinition.IsLasting`;
      - `ResourceGains.PerBattleFought`.
      - `Party/PartyMember`:
        - `LastingAmounts` (`IReadOnlyDictionary<string, int>`), set from the class at `Recruit` and `Restore`;
        - `internal void KeepLastingAmount(string resourceId, int amount)`.
      - `Battle/ResourceStarts` starts a lasting resource from the member's amount.
      - `Session/GameSession.FinishBattle` calls `LastingResourceCarry.CarryBack` before granting experience.
  - **Tests:**
    - `PartyMemberTests`:
      - `Recruit_ClassWithLastingResource_StartsAtStartsAt`
      - `KeepLastingAmount_AboveMax_Clamps`
    - `CombatantTests`: `FromPartyMember_LastingResource_StartsAtMembersAmount`
    - `GameSessionBattleTests`:
      - `FinishBattle_Victory_CarriesLastingAmountBack`
      - `FinishBattle_Escape_CarriesBackAndAppliesPerBattle`
      - `FinishBattle_NonLastingResource_NotCarried`
      - `FinishBattle_PerBattleFought_AppliedOncePerBattle`
  - **Verify by hand:** none (no content until LR3).
