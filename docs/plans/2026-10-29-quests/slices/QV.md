# QV. The quest condition in content, and no shadowing event

Part of [the quests plan](../README.md). After QC. Decision 389. Design: [design.md](../design.md), "What exists" (Data: `AbsentWhenNullFilter`), "Key decisions" (no shadowing); README "Planner's picks" (the shadowing rule's scope).

- **QV. The condition in content, and no event shadowing the later ones on its object** (Data, about 210 lines)
  - **Goal:** a map's event reads `"onlyWhenQuest": { "questId": "...", "kind": "atStage", "stageId": "..." }`; content is refused when the condition names an unknown quest or stage, names a stage with a kind that takes none, or lacks one with a kind that needs one; an object event that can always start (no object, leader or quest gate, not "once") before another event of the same activation on the same object is refused, since that one could never run; and every existing map file writes back byte for byte.
  - **New types:** none.
  - **Changes:**
    - `Data/Maps/MapEventValidator.FindStartProblem`: a helper `FindQuestConditionProblem(condition, references)` (CA1502): `onlyWhenQuest has unknown quest id 'x'.`, `onlyWhenQuest kind 'active' names no stage; remove stageId 'y'.`, `onlyWhenQuest kind 'atStage' needs a stageId.`, `onlyWhenQuest has unknown stage 'y' of quest 'x'.`
    - `Data/Maps/MapEventValidator`: `FindShadowedEvent(layout)` (about 20 lines, called beside `FindRepeatedEventId`): `object 'uid': event 'a' can always start, so the later interact event 'b' never would; gate it or make it once.`
    - `Data/Content/AbsentWhenNullFilter`: `(MapEvent, "onlyWhenQuest")` and `(QuestCondition, "stageId")`, each with its one-line reason, as the two entries there.
  - **Tests** (`MapEventValidatorQuestTests`, both sides of each rule):
    - `FindProblem_ConditionUnknownQuest_NamesIt` / `FindProblem_ConditionKnownQuest_ReturnsNull`
    - `FindProblem_ActiveWithStage_SaysNoStage` / `FindProblem_ActiveWithoutStage_ReturnsNull`
    - `FindProblem_ReachedStageWithoutStage_SaysItNeedsOne` / `FindProblem_ReachedStageWithStage_ReturnsNull`
    - `FindProblem_ConditionUnknownStage_NamesQuestAndStage` / `FindProblem_ConditionKnownStage_ReturnsNull`
    - `FindProblem_UngatedEventBeforeAnotherOfItsActivation_NamesBoth` / `FindProblem_UngatedEventLast_ReturnsNull`
    - `FindProblem_UngatedOnceEventFirst_ReturnsNull` (once is the other way out)
    - `ContentWriterQuestTests: WriteMap_EventWithoutQuestCondition_LeavesTheFieldOut`
    - `ContentWriterQuestTests: WriteMap_ConditionWithoutStage_LeavesStageIdOut`
  - **Watch:** the repository's maps must pass the shadowing rule (planned: the door's two events differ in activation and are both gated; the intro's leader-gated events are cell events; every other object has one event) and write back unchanged (the existing byte-for-byte writer test).
  - **Known gap:** until E2, pressing OK in an event dialog drops an event's `onlyWhenQuest` without a word (`EventViewModel.ToEvent` doesn't carry it yet); no content has one before N1.
  - **Verify by hand:** nothing to see yet.
