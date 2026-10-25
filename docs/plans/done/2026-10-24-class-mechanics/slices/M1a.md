# M1a. Combatants carry their class's resources

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Core changes" (Resources, Battle), "When things happen in a turn" (step 4). ADR 0019.

- **M1a. Combatants carry their class's resources and gain at the end of each of their turns** (Core, about 230 lines)
  - **Goal:**
    - A class can have resources. A member enters every battle with each at its starting amount, gains its per-turn amount at the end of each of their own turns (within 0..max), and each non-zero change is reported as `ResourceChanged`.
    - A new battle starts afresh.
    - No content has a resource yet (M1b), so play doesn't change.
  - **New types:**
    - `Resources/ResourceDefinition` (public sealed record): one battle resource a class has: `Id`, `Name`, `Max`, `StartsAt`, `Gains`.
    - `Resources/ResourceGains` (public sealed record): how much a resource gains on each trigger; here only `PerTurn` (default 0).
    - `Resources/ResourceMeters` (public sealed class): one combatant's current resource amounts in one battle, within 0..max.
      - `ResourceMeters(IReadOnlyList<ResourceDefinition> definitions)`; `Definitions`;
      - `AmountOf(string resourceId)`, which throws `ArgumentException` naming an unknown id;
      - `internal int Change(string resourceId, int amount)` returns the change applied after clamping.
    - `Battle/ResourceChanged(Combatant Holder, string ResourceId, int Change)` (BattleEvent).
    - `Battle/TurnUpkeep` (internal static): the end of a combatant's turn; here `ApplyPerTurnGains(Combatant actor, List<BattleEvent> events)`.
  - **Changes:**
    - `Party/ClassDefinition` gains `IReadOnlyList<ResourceDefinition> Resources` (empty when absent).
    - `Battle/Combatant.Resources`: `FromPartyMember` builds it from `member.Class.Resources`; `FromEnemy` with none.
    - `Battle/BattleState.EndTurn` runs `TurnUpkeep` before the outcome check.
    - Tests: `ClassBuilder.WithResources(params ResourceDefinition[])`.
  - **Tests** (about 13 lines each):
    - `ResourceMetersTests`:
      - `Constructor_Definitions_StartAtStartsAt`
      - `Change_AboveMax_ClampsAndReturnsApplied`
      - `Change_BelowZero_ClampsAndReturnsApplied`
      - `AmountOf_UnknownId_Throws`
    - `BattleStateResourceTests` (new file):
      - `Start_MemberWithResource_StartsAtStartsAt`
      - `Execute_EndOfMemberTurn_GainsPerTurnAndReportsIt`
      - `Execute_AtMax_ReportsNoChange`
      - `Execute_EnemyTurn_MemberGainsNothing`
      - `Start_SecondBattle_StartsAtStartsAgain`
    - `CombatantTests`: `FromEnemy_Always_HasNoResources`
  - **Verify by hand:** none (no content yet); `GoblinTrioSimulation` still passes.
- **Done, with deviations:** as planned. Tests 1787 before, 1797 after. `ResourceGains` is a record whose `PerTurn` property defaults to 0, so that later triggers are added as optional fields. `EndTurn` now takes the actor.
