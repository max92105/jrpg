# WD6. The Effect strips page

**Stage 2.** Recipe: [page-recipe.md](../page-recipe.md), "A window to convert" (step 0 first; row 7 later puts its
shared preview-and-pace control on this page).

**Goal:** the Effect strips window becomes a page in `CatalogDialog`, its preview playing on the page's own
`RenderTick` from `Loaded` to `Unloaded`.

## New page
- `Battle/EffectStripCatalogPage` (user control): the window's layout and code-behind without OK and Cancel; its image
  cache lives as long as the page.

## Changes
- `Battle/EffectStripCatalogViewModel`: implements `IEditedCatalog` (`ChosenKey` = `SelectedStrip?.File`, `Choose` by
  file, an unlisted or missing image included).
- `Shell/MainWindow.Database.cs`: the opener through `OpenInDialog`, the page built with the effects image loader and
  the animation clock.
- `HasEffectStripEditing` and its binding go.
- Deleted: `Battle/EffectStripCatalogWindow.xaml`, `.xaml.cs`. `docs/map.md`: the Battle row.

## Tests
`tests/Jrpg.Editor.Tests/Battle/EffectStripCatalogViewModelTests.cs` (210 → about 240), strips built in the test:
- `EffectStripCatalogViewModelTests: Choose_UnlistedImage_ChoosesIt`
- `EffectStripCatalogViewModelTests: Choose_UnknownFile_KeepsTheChoice`
- `EffectStripCatalogViewModelTests: ChosenKey_ChosenStrip_IsItsFile`

## Verify by hand
Agents may run it (effects are generated art). The script: "Database>Effect strips...: opened", the strip playing. In
the editor: type a frame time: the pace changes; Esc: closes with no `RenderTick` report; reopen: the strip unchanged.
- **Done, with deviations:** `CanApply` removed (the window was its only reader): `CanApply_WithAProblem_IsFalse`
  renamed `Problem_HitFramePastTheImage_IsTheChosenStrips`, its assert on the problem's words; the test helper
  `Window()` and its locals renamed `Catalog()` and `catalog`; the test file was 210 lines, now 242.
