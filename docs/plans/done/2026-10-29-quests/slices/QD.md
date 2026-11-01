# QD. Quest definitions in `quests.json`

Part of [the quests plan](../README.md). First slice. Decision 397 (the placeholder). Design: [design.md](../design.md), "Core changes", "Structure and folder dependencies".

- **QD. Quest definitions in `quests.json` with the placeholder errand, validated and in the catalog** (Core + Data + content, about 230 lines)
  - **Goal:** the game loads `content/quests.json` into `ContentCatalog.Quests` and refuses a malformed quest naming the file, the quest and the stage. The file holds the placeholder errand from here on, so every later slice tests and shows a real quest.
  - **New types:**
    - `Core/Quests/QuestDefinition` (public sealed record): one quest as content. `Id`, `Title`, `Description`, `Kind`, `Stages` (`IReadOnlyList<QuestStage>`, in order). `FirstStage`, `LastStage`, `HasStage(string stageId)`, `Stage(string stageId)` (throws `ArgumentException` naming both ids on an unknown stage: content is validated, so only a bug reaches it).
    - `Core/Quests/QuestStage` (public sealed record): one stage as content: `Id` and the `Text` the page shows while the quest is at it.
    - `Core/Quests/QuestKind` (public enum): `Main`, `Side`; read as `"main"`/`"side"` by the content options' string enum converter.
    - `Data/Quests/QuestCatalogValidator` (internal static): `quests.json`'s rules, on `ContentListValidator<QuestDefinition>`: title and description not blank, at least one stage, no null stage, stage ids not blank and not repeated within the quest, stage text not blank. Messages as `quests.json: quest 'x': stage 'y' ...`.
  - **Changes:**
    - `Core/Content/ContentCatalog.Quests` (required, `IReadOnlyDictionary<string, QuestDefinition>`, with a one-line summary).
    - `Data/Content/ContentCatalogLoader`: `QuestsFile = "quests.json"` (internal, QE1 writes it), read and validated before the maps (QS's map checks need it), put in the catalog.
    - `tests/Jrpg.Core.Tests/Builders/ContentCatalogBuilder`: `WithQuest(QuestDefinition)`, `Quests` in `Build` (the only other construction site).
    - `content/quests.json`: `placeholder_errand`, side, title "Placeholder errand", a one-line description (placeholder), stages `ask_innkeeper` ("Ask the innkeeper about the road north.") and `return_to_giver` ("Bring the innkeeper's answer back."). Nothing uses it until N1.
  - **Tests:**
    - `QuestCatalogValidatorTests: Validate_WellFormedQuest_Passes`
    - `QuestCatalogValidatorTests: Validate_NoStages_ThrowsNamingTheQuest`
    - `QuestCatalogValidatorTests: Validate_NullStage_ThrowsNamingTheQuest`
    - `QuestCatalogValidatorTests: Validate_RepeatedStageId_ThrowsNamingTheStage`
    - `QuestCatalogValidatorTests: Validate_BlankStageText_ThrowsNamingTheStage`
    - `QuestCatalogValidatorTests: Validate_BlankTitle_ThrowsNamingTheQuest`
    - `QuestCatalogValidatorTests: Validate_RepeatedQuestId_ThrowsNamingIt`
    - `QuestDefinitionTests: Stage_UnknownStageId_ThrowsNamingBothIds`
    - `ContentCatalogLoaderQuestTests: Load_RepositoryContent_HasThePlaceholderErrandWithItsStagesInOrder` (new file: `ContentCatalogLoaderTests` is at 272)
    - `ContentCatalogLoaderQuestTests: Load_UnknownQuestKind_ThrowsNamingTheFile` (a copied content root)
  - **Call sites:** none beyond the builder.
  - **Verify by hand:** start the game; it starts as before. Break `quests.json` (an empty stage list): the start stops naming the file and the quest; put it back.

- **Done, with deviations:** the builder's method is `WithQuests(params QuestDefinition[])`, the shape of its siblings, not `WithQuest`. `QuestDefinition` compares its stages by sequence (as `ShopDefinition` does its ids), tested. The validator's cases are one theory (`InvalidQuests`, a null stage list included) plus the repeated id and the well-formed quest. The errand's second stage avoids an apostrophe, which a catalog writer would escape. Tests 3146 → 3163. Review: the loader test copies the content through a shared `CopiedContentRoot` fixture (the four older copies go to the backlog).
