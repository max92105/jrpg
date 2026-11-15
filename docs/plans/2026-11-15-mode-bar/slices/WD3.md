# WD3. The Quests page

**Stage 2**, right after stage 1. Recipe: [page-recipe.md](../page-recipe.md), "A window to convert" (step 0 first:
re-read the window and recount).

**Goal:** the Quests window becomes a page in `CatalogDialog`; its problem now shows live under the page (the window
showed it only on OK).

## New page
- `Quests/QuestCatalogPage` (user control): the window's layout without OK, Cancel and `ProblemText`.

## Changes
- `Quests/QuestCatalogViewModel`: implements `IEditedCatalog` (`ChosenKey` = `SelectedQuest?.Id`, `Choose`); its
  `Problem` is already raised on every quest change.
- `Shell/MainWindow.Database.cs`: the Quests opener through `OpenInDialog`; `Shell/MainWindow.QuestEditing.cs` deleted.
- `HasQuestEditing` and its binding go.
- Deleted: `Quests/QuestCatalogWindow.xaml`, `.xaml.cs`. `docs/map.md`: the Quests row.

## Tests
`tests/Jrpg.Editor.Tests/Quests/QuestCatalogViewModelTests.cs` (98 → about 130), quests built in the test:
- `QuestCatalogViewModelTests: Choose_KnownId_ChoosesIt`
- `QuestCatalogViewModelTests: Choose_UnknownId_KeepsTheChoice`
- `QuestCatalogViewModelTests: ChosenKey_NothingChosen_IsNull`

## Verify by hand
Agents may run it. The script: "Database>Quests...: opened", main-window size. In the editor: a stage with an empty id
shows its problem at once and greys OK; OK after fixing it, Ctrl+Z takes the session back.
