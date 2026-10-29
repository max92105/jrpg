# QD. Quest definitions in `quests.json`

Part of [the quests plan](../README.md). First slice. Design: [design.md](../design.md), "Core changes" and "Structure and folder dependencies".

- **QD. Quest definitions in `quests.json`, validated and in the catalog** (Core + Data + content, about 220 lines)
  - **Goal:** the game loads `content/quests.json` into `ContentCatalog.Quests`, and refuses a malformed quest naming the file, the quest and the stage. The file starts as `[]`: N1 writes the placeholder quest.
  - **New types:**
    - `Core/Quests/QuestDefinition` (public sealed record): one quest as content. `Id`, `Title`, `Description`, `Kind`, `Stages` (`IReadOnlyList<QuestStage>`, in order). `FirstStage`, `LastStage`, `HasStage(string stageId)`, `Stage(string stageId)` (throws `ArgumentException` naming both ids on an unknown stage: content is validated, so only a bug reaches it).
    - `Core/Quests/QuestStage` (public sealed record): one stage as content: `Id` and the `Text` the page shows while the quest is at it.
    - `Core/Quests/QuestKind` (public enum): `Main`, `Side`. Read as `"main"`/`"side"` by the content options' string enum converter.
    - `Data/Quests/QuestCatalogValidator` (internal static): `quests.json`'s rules, on `ContentListValidator<QuestDefinition>`: title and description not blank, at least one stage, no null stage, stage ids not blank and not repeated within the quest, stage text not blank. Messages as `quests.json: quest 'x': stage 'y' ...`.
  - **Changes:**
    - `Core/Content/ContentCatalog.Quests` (required, `IReadOnlyDictionary<string, QuestDefinition>`, with a one-line summary).
    - `Data/Content/ContentCatalogLoader`: `QuestsFile = "quests.json"`, read and validated before the maps (QS's map checks need it), put in the catalog.
    - `tests/Jrpg.Core.Tests/Builders/ContentCatalogBuilder`: `WithQuest(QuestDefinition)`, `Quests` in `Build` (the only other construction site).
    - `content/quests.json`: `[]`.
  - **Tests:**
    - `QuestCatalogValidatorTests: Validate_WellFormedQuest_Passes`
    - `QuestCatalogValidatorTests: Validate_NoStages_ThrowsNamingTheQuest`
    - `QuestCatalogValidatorTests: Validate_NullStage_ThrowsNamingTheQuest`
    - `QuestCatalogValidatorTests: Validate_RepeatedStageId_ThrowsNamingTheStage`
    - `QuestCatalogValidatorTests: Validate_BlankStageText_ThrowsNamingTheStage`
    - `QuestCatalogValidatorTests: Validate_BlankTitle_ThrowsNamingTheQuest`
    - `QuestCatalogValidatorTests: Validate_RepeatedQuestId_ThrowsNamingIt`
    - `QuestDefinitionTests: Stage_UnknownStageId_ThrowsNamingBothIds`
    - `ContentCatalogLoaderQuestTests: Load_QuestsFile_ReadsKindAndStagesInOrder` (a copied content root with one quest written in; new file: `ContentCatalogLoaderTests` is at 272)
    - `ContentCatalogLoaderQuestTests: Load_UnknownQuestKind_ThrowsNamingTheFile`
  - **Call sites:** none beyond the builder (the loader and the builder are the only `ContentCatalog` constructions).
  - **Verify by hand:** start the game; it starts as before. Put `{}` in `quests.json`: the start stops naming `quests.json`; put it back.
