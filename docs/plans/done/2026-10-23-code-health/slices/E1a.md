# E1a. `MainViewModelTests` by area: shared content copy, saving and undo classes

Part of [the code health plan](../README.md). Design: [design.md](../design.md), backlog survey ("`MainViewModelTests` 777"), key decision 3; risk "Moved tests that silently stop running".

- **E1a. `MainViewModelTests` by area** (Editor tests, about 80 lines plus about 330 moved)
  - **Goal:**
    - The save tests and the undo tests each live in their own class, sharing one content-copy helper.
    - `MainViewModelTests` drops to about 450 lines. The rest of the split (new content, map settings) waits in the backlog.
    - The test count of `Jrpg.Editor.Tests` is the same before and after; record both.
  - **New types** (tests, `tests/Jrpg.Editor.Tests/Shell/`):
    - `EditorTestContent` (test helper, `IDisposable`): a temporary copy of the repository's content, and the questions the view model asked.
      - `string RepositoryContent`, `string ContentCopy`, `List<string> Questions`.
      - `string CopyOfRepositoryContent()`, `MainViewModel Open(string contentRoot, bool answer = false)`.
      - The current private step helpers, moved unchanged: `PaintTopLeft`, `ChooseTwoCellsNotTiles`, `ChooseCellNotATile`, `BlockFirstWalkableTile`, `TilesAndForestBytes`.
    - `MainViewModelSaveTests`: the 8 `Save_*` tests.
    - `MainViewModelUndoTests`: the 8 `Undo_*`/`Redo_*` tests.
  - **Changes:**
    - Each class holds `private readonly EditorTestContent _content = new();` and disposes it.
    - Moved bodies keep their statements. The only edit is the mechanical prefix: `Open(` becomes `_content.Open(`, `_questions` becomes `_content.Questions`, and the same for the other shared names. There are no permanent forwarding members.
    - `MainViewModelTests` uses the helper the same way.
    - `docs/plans/backlog.md`: the `MainViewModelTests` line becomes the part-2 line under "When next touched" (new content, map settings).
  - **Tests:** none new.
  - **Verify by hand:** the `Jrpg.Editor.Tests` count before and after, written here.
- **Done, with deviations:** `Jrpg.Editor.Tests`: 402 tests before, 402 after. Split by a script like X1's (each test put in its class by its name, in its old order); moved bodies changed only by the prefix: `_content.` for `Open`, `Questions`, `ContentCopy`, `RepositoryContent`, `CopyOfRepositoryContent` and the three helpers that read the copy, `EditorTestContent.` for the two static helpers (`PaintTopLeft`, `BlockFirstWalkableTile`). Sizes: `MainViewModelTests` 449 (was 777), `MainViewModelSaveTests` 132, `MainViewModelUndoTests` 144, `EditorTestContent` 101. Review: no findings; every moved body matches HEAD apart from the prefix.
