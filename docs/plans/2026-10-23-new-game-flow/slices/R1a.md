# R1a. A map event can recruit a member

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Recruiting", key decision 5.

- **R1a. A map event can recruit a member** (Core + Data + Game + content, about 230 lines; decision 163; question 179)
  - **Goal:**
    - An event step `{ "type": "recruit", "memberId": "omer" }` adds that member to the party at level 1, 0 XP, full HP and MP.
    - The member goes to the active party when there's room, else to the reserve.
    - The player reads "Omer joins the party.".
    - If the member is already recruited, nothing changes, and the player reads "Omer is already in the party.".
  - **New types (Core):** `Events/RecruitStep(string MemberId) : EventStep`, which adds a party member, then waits while the player reads who joined.
  - **Changes:**
    - Core:
      - `Party/PartyState.Recruit(PartyMember member)`: adds the member, into the active party while it has room; throws if already recruited. `IsRecruited(string memberId)` is the query.
      - `EventRun` (or where steps run, ADR 0006) handles `RecruitStep`. It builds the member with `PartyMember.Recruit` from the catalog's definition and class, and raises an outcome the Game shows, as `GiveItemStep` does.
    - Data:
      - `ContentTypeDiscriminators` gains `recruit`;
      - `MapEventValidator` checks that `memberId` is a known member.
    - Game: the event's notice shows "{name} joins the party." or "{name} is already in the party." (`ui.json`, `MessageIds`), through the path that shows "received" today.
    - The editor must open maps that use the step. R1b adds its editing; until then, no repository map uses it, so the editor's throw on unknown steps isn't reached.
  - **Tests:**
    - `PartyStateTests`:
      - `Recruit_RoomInActiveParty_JoinsActive`
      - `Recruit_ActivePartyFull_JoinsReserve`
      - `Recruit_AlreadyRecruited_Throws`
    - `EventRunTests`:
      - `Run_RecruitStep_AddsMemberAtLevelOne`
      - `Run_RecruitStepForRecruited_ChangesNothingAndSaysSo`
    - `MapEventValidatorTests`: theory row for an unknown `memberId`.
    - `SaveGameStoreTests.RoundTrip_RecruitedDuringPlay_KeepsThem`
  - **Verify by hand:** covered by R2, which puts recruit points on the start map.
- **Done, with deviations:**
  - `ExplorationState` takes the party instead of the leader id, since the leader is the party's fixed first member. `EventRun` takes the exploration context and the party, so neither constructor goes past 4 parameters. Test sites use a new `TestParties.LedBy(id)`.
  - What a recruit step did is `Exploration/Recruitment(Member, HasJoined)`. Its message comes from `Dialogue/RecruitText`, beside `FoundItemText`.
  - The save round trip is `GameSessionSaveTests.CaptureThenRestore_RecruitedDuringPlay_KeepsThem` (no `SaveGameStoreTests` exists, and the format is unchanged). Tests: 1739 before, 1746 after.
