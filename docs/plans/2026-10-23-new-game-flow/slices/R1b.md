# R1b. The editor authors the recruit step

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Structure" (Editor).

- **R1b. The editor authors the recruit step** (Editor, about 120 lines; decision 163)
  - **Goal:** in the event dialog, "Recruit" is a step kind with a member picker. A map holding a recruit step opens, edits and saves without loss.
  - **Changes (Editor):**
    - `Events/EventStepKind.Recruit`.
    - `Events/EventStepViewModel`:
      - a `MemberId` field;
      - `From(RecruitStep)` and `ToStep()` cases;
      - the label shown in the step list ("Recruit Omer").
    - The step editor's XAML: a member combo box, shown for the Recruit kind, as the item picker is shown for Give item. The members come from the catalog the dialog already gets.
    - `Events/EventPresets`: a "Recruit a member" preset if presets list one per step kind. Otherwise none.
  - **Tests (`EventStepViewModelTests`):**
    - `From_RecruitStep_KeepsTheMember`
    - `ToStep_RecruitWithMember_BuildsRecruitStep`
    - `ToStep_RecruitWithoutMember_IsInvalid`
  - **Verify by hand:** open the start map after R2 and open a recruit point's event: it shows "Recruit Omer". Change the member, save, reopen: the change is kept.
- **Done, with deviations:** The step list reads "Recruit: omer", using the id like the other steps' labels ("Give item: potion x2"), not "Recruit Omer". Both event windows gain a "Recruit" add button. No preset: the presets are per object kind, not per step. An empty member is a `Problem` ("Choose the member who joins."), as for the other steps, and `ToStep` is not reached while a problem shows. The every-kind round-trip theory gains a recruit step. No screenshot (privacy rule); the dialog check is the developer's. Tests: 1746 before, 1750 after.
