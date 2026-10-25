# PL2a. Guarded; current stats

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Guarded), "Replacement mapping" (`CurrentStats`). ADR 0021.

- **PL2a. Guarded raises defense; combatants have current stats** (Core, about 170 lines)
  - **Goal:**
    - A combatant's current stats are its stats with its statuses applied. Guarded adds its power to defense while it lasts.
    - Attacks and skills read current stats: the attacker's attack, the target's defense.
    - **Known gap:** no content grants Guarded until PL2b.
  - **New types:** Core `Statuses/StatusModifiers` (public static): what a combatant's statuses do to its stats; `Apply(CombatStats stats, StatusSet statuses)`. Here only Guarded counts.
  - **Changes:**
    - Core:
      - `Statuses/StatusKind.Guarded`;
      - `Battle/Combatant.CurrentStats`;
      - `Battle/Combatant.Stats`'s summary: the base for the battle, before statuses;
      - `BattleState.Attack` and `ApplyToTargets` read `CurrentStats`.
    - Game: `ui.json` and `MessageIds`: `status_guarded` ("Guard").
  - **Tests:**
    - `StatusModifiersTests`:
      - `Apply_Guarded_AddsPowerToDefense`
      - `Apply_NoStatus_ReturnsStatsUnchanged`
    - `BattleStatusTests`:
      - `Execute_AttackOnGuardedTarget_UsesRaisedDefense`
      - `Execute_GuardedEnds_DefenseBackToBase`
    - `CombatantTests`: `CurrentStats_NoStatus_EqualsStats`
  - **Verify by hand:** none (PL2b).
