# WD7. The Cinematics page

**Stage 2.** Recipe: [page-recipe.md](../page-recipe.md), "A window to convert" (step 0 first: row 4's focus point has
changed this window and its code-behind since 2026-11-15).

**Goal:** the Cinematics window becomes a page in `CatalogDialog`; Play stops when the page goes (`Unloaded` runs what
`Closed` ran).

## New page
- `Cinematics/CinematicCatalogPage` (user control): the window's layout and code-behind without OK and Cancel;
  `PreviewWidth` (still a `double`: an `x:Static` size reads one) moves to it.

## Changes
- `Cinematics/CinematicCatalogViewModel`: implements `IEditedCatalog` (`ChosenKey` = `SelectedCinematic?.Id`,
  `Choose`).
- `Shell/MainWindow.Database.cs`: the opener through `OpenInDialog`, the picture loader reading
  `CinematicCatalogPage.PreviewWidth`.
- `HasCinematicEditing` and its binding go.
- Deleted: `Cinematics/CinematicCatalogWindow.xaml`, `.xaml.cs`. Grep "Cinematics window" in `Cinematics/` comments and
  `docs/map.md`: "Cinematics page".

## Tests
`tests/Jrpg.Editor.Tests/Cinematics/CinematicCatalogViewModelTests.cs` (136 → about 165), cinematics built in the test:
- `CinematicCatalogViewModelTests: Choose_KnownId_ChoosesIt`
- `CinematicCatalogViewModelTests: Choose_UnknownId_KeepsTheChoice`
- `CinematicCatalogViewModelTests: ChosenKey_NewCinematicTyped_IsItsTypedId`

## Verify by hand
Agents may run it (the stills are generated art). The script: "Database>Cinematics...: opened". In the editor: Play,
then Esc while it plays: no error box, no `RenderTick` report; the focus point still picks on the picture (row 4).
