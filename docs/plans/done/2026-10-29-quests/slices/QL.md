# QL. The quest log in `WorldState`

Part of [the quests plan](../README.md). After QD and ADR 0032. Decision 388 (steps out of order). Design: [design.md](../design.md), "Core changes", "Key decisions" (stage ids; the log takes the definition).

- **QL. The quest log in `WorldState`: start, advance, complete** (Core, about 230 lines)
  - **Goal:** Core holds each started quest's stage and completion, changed only through three commands whose rules for every state are Core's and tested.
  - **New types:**
    - `Core/Quests/QuestLog` (internal sealed class): each started quest with its definition, its stage and whether it is completed (a private record per quest).
      - `string? StageOf(string questId)` (null while not started); `bool IsCompleted(string questId)`.
      - `void Start(QuestDefinition quest)`: a quest not started becomes active at its first stage; otherwise nothing.
      - `void AdvanceTo(QuestDefinition quest, string stageId)`: an active quest moves to that stage, an earlier one included; a quest not started starts at it; a completed one stays as it is. An unknown stage throws (`quest.Stage`).
      - `void Complete(QuestDefinition quest)`: an active quest is completed at its stage; a quest not started is completed at its last stage; a completed one stays.
      - No start order yet: SV adds it with `Progress`, its first reader (a field written and never read fails the build, IDE0052).
  - **Changes:** `Core/World/WorldState.Quests` (`QuestLog`, new per world); the class summary names the quests beside the events and swaps.
  - **Tests** (`QuestLogTests`, a two-stage quest from a small local factory):
    - `Start_NotStarted_ActiveAtFirstStage`
    - `Start_Active_KeepsItsStage`
    - `Start_Completed_StaysCompleted`
    - `AdvanceTo_Active_MovesToTheStage`
    - `AdvanceTo_EarlierStage_MovesBack`
    - `AdvanceTo_NotStarted_StartsAtTheStage`
    - `AdvanceTo_Completed_StaysCompletedAtItsStage`
    - `AdvanceTo_UnknownStage_Throws`
    - `Complete_Active_CompletedAtItsStage`
    - `Complete_NotStarted_CompletedAtLastStage`
    - `Complete_Completed_StaysCompleted`
    - `StageOf_NeverStarted_IsNull`
    - `WorldStateTests: Quests_NewWorld_NoQuestStarted`
  - **Call sites:** none (`new WorldState()` unchanged).
  - **Verify by hand:** nothing to see yet; the tests are the check.

- **Done, with deviations:** the log's entry keeps only the stage id and the completion, not the definition: nothing reads a kept definition before QC and SV, and an unread member fails the build; QC adds it with its first reader. `AdvanceTo` resolves the stage before the completed check, so an unknown stage throws in every state. Tests 3163 → 3176 (12 log tests, one world test).
