# RS1. The rest step

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Rest), "Core changes" (Events). Decision 198; questions 221, 222. ADR 0024, ADR 0006.

- **RS1. The rest step: an event rests the party** (Core + Data + Game, about 240 lines)
  - **Goal:**
    - A map event step rests the whole party, reserves included (question 221). Each member:
      - gets full HP, and the fallen are revived;
      - has every lasting resource refilled to its starting amount.
    - The step shows "The party rests." and waits for Continue, as give-item does.
    - **Known gap:** no map uses it until RS2, because the editor must open such maps first.
  - **New types:** Core `Events/RestStep` (public sealed record, `EventStep`): the party rests.
  - **Changes:**
    - Core:
      - `Party/PartyMember.Rest()` (public, since `FieldItemUse` in RS3 and `EventRun` both call it): full HP through `Vitals`, revive, and lasting amounts refilled.
      - `Exploration/EventRun` runs `RestStep` on every recruited member, then waits.
    - Data: the step reader and writer (`Maps/MapEventSteps`, the step discriminator) learn `rest`; nothing to validate.
    - Game:
      - `Exploration/EventPresenter` shows the rest line through the dialogue box;
      - `ui.json` and `MessageIds`: `event_party_rests`.
  - **Tests:**
    - `PartyMemberTests`:
      - `Rest_Wounded_FullHp`
      - `Rest_Fallen_Revived`
      - `Rest_LastingSpent_RefilledToStartsAt`
    - `EventRunTests`:
      - `Advance_RestStep_RestsEveryRecruitedMember`
      - `Advance_RestStep_WaitsForContinue`
    - `MapEventStepsTests` (or the file that round-trips steps): `ReadWrite_RestStep_RoundTrips`
    - `EventPresenterTests`: `Update_RestStep_ShowsRestLineThenContinues`
  - **Verify by hand:** none until RS2.
