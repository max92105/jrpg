# CO3. ObjectComposerViewModel and ObjectNaming: what Save makes

Builds on design.md, "Key decisions" 2, and the README's picks 1, 5 and 8.

## Goal
The composer's state around the painting: for a new object, the id and category typed in the composer itself; for an edited one, its id shown fixed and its category editable; what stops Save (the size, the id, the category, nothing painted), and the object Save makes: a new one with no outline and no zones, or the original with its new size, tiles and category, keeping its id, outline and zones. Whether the Shapes window should open after Save: always for a new object, for an edited one only when its size changed.

## New types
- `Editor/Objects/ObjectNaming` (`internal sealed partial class : ObservableObject`): the id and category typed for a new object, with their problems. `NewObjectViewModel`'s rules without its cells: `ObjectNaming(IReadOnlyCollection<string> takenIds, IReadOnlyList<string> categories)`, `Id`, `Category`, `Categories`, `IdProblem` (`ObjectIds.ProblemOf`), `CategoryProblem`. `NewObjectViewModel` stays until CO5 removes it with its window (a two-slice overlap, not a copy kept).
- `Editor/Objects/Composing/ObjectComposerViewModel` (`internal sealed class`): the composer window's state: the painting, the naming of a new object or the edited original, what stops Save, and the object it makes.
  - `static ObjectComposerViewModel ForNew(CanvasPainting painting, ObjectNaming naming)`; `static ObjectComposerViewModel ForEdit(CanvasPainting painting, ObjectDefinition original, IReadOnlyList<string> categories)` (its naming holds the original's category, its id fixed).
  - `CanvasPainting Painting`; `ObjectNaming Naming`; `ObjectDefinition? Original`; `bool IsIdEditable`; `string Title` ("New object", "Edit object 'chest'").
  - `string? Problem` (first of `Painting.SizeProblem`, the naming's problems while new, "Paint at least one cell."), raised when any of them changes.
  - `bool ShouldOpenShapesAfterSave` (new, or the canvas size differs from `Original`'s).
  - `ObjectDefinition ToObject(Func<SheetCell, int> tileIdOf)`: throws `InvalidOperationException` while `Problem` isn't null.

## Changes
None outside the new types.

## Tests (new `Objects/ObjectNamingTests`, `Objects/Composing/ObjectComposerViewModelTests`)
- `ObjectNamingTests`: the cases of `NewObjectViewModelTests` but its size text, rewritten on the new type (`IdProblem_TakenId_SaysSo`, `IdProblem_ValidUnusedId_IsNull`, `CategoryProblem_Blank_SaysSo`, `Categories_Duplicates_ListedOnceSorted`).
- `ObjectComposerViewModelTests: Problem_NothingPainted_AsksForACell`.
- `ObjectComposerViewModelTests: Problem_NewWithATakenId_IsTheIdProblem`.
- `ObjectComposerViewModelTests: Problem_EditWithItsOwnId_IsNull` (its id is taken by itself).
- `ObjectComposerViewModelTests: ToObject_New_HasTheTypedIdTrimmedCategoryNoOutlineNoZones`.
- `ObjectComposerViewModelTests: ToObject_EditOtherSize_KeepsIdOutlineAndZonesTakesSizeTilesAndCategory`.
- `ObjectComposerViewModelTests: ToObject_WithAProblem_Throws`.
- `ObjectComposerViewModelTests: ShouldOpenShapesAfterSave_EditSameSize_IsFalse` (and `_EditOtherSize_IsTrue`, `_New_IsTrue`).

Size: about 80 lines of code, 12 tests (about 190).

## Verify by hand
Nothing to see yet (CO5).
