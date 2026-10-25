# DR1. Statuses that tick: poison and regeneration

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Poison, Regen), "When things happen in a turn" (step 4). ADR 0021.

- **DR1. Statuses that tick: poison and regeneration at the end of the holder's turn** (Core + Game playback, about 250 lines)
  - **Goal:**
    - At the end of its holder's regular turn, before the per-turn gains and the countdown:
      - Poison takes its power in HP, ignoring defense and Exposed. It can fell the holder: a fall like any other, with experience, defeat, conviction, faith and cleared statuses.
      - Regen restores its power in HP.
    - A lost (stunned) turn ticks too. An extra (readied) turn doesn't, since it has no upkeep.
    - A fresh status ticks on its first turn end: only its countdown waits.
    - The screen shows each tick as a floating number on the holder, in a `Tick` beat after the impact.
    - **Known gap:** no content grants these until DR2.
  - **New types:**
    - Core `Battle/StatusTicked(Combatant Holder, StatusKind Status, int Amount)` (BattleEvent): HP lost to Poison or restored by Regen, as applied.
  - **Changes:**
    - Core:
      - `Statuses/StatusKind.Poison` and `Regen`.
      - `Battle/TurnUpkeep.TickStatuses(Combatant holder, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)`, in the order the statuses were applied. A fall reports `CombatantDefeated` and runs `ResourceTriggers.OnCombatantFallen`.
      - `BattleState.EndTurn`: an actor felled by its own poison skips the rest of its upkeep; the outcome check runs as usual.
    - Game:
      - `PlaybackBeatKind.Tick`;
      - `ActionPlayback` handles `StatusTicked`: a `FloatingNumber` with the damage or heal kind, and a falling holder joins the defeat beat;
      - `ui.json` and `MessageIds`: `status_poison` ("Poison") and `status_regen` ("Regen").
  - **Tests:**
    - `BattleStatusTests`:
      - `Execute_PoisonedActorEndsTurn_LosesPowerIgnoringDefense`
      - `Execute_PoisonFellsLastEnemy_Victory`
      - `Execute_PoisonFellsActor_NoGainOrCountDownAfter`
      - `Execute_RegenAtFullHp_ReportsZero`
      - `Execute_StunnedAndPoisoned_TicksOnSkippedTurn`
      - `Execute_PoisonFellsEnemy_PaladinGainsConviction`
    - `ActionPlaybackTests`:
      - `Start_StatusTicked_AddsTickBeatAfterImpact`
      - `Start_PoisonTickFells_AddsDefeatBeat`
  - **Verify by hand:** none (DR2).
