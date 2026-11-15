# EE3. The Effect strips window's rules: every effects image, unlisted or not, reset, one undo step

Editor, about 210 lines (about 110 of them tests). Needs EE2. Decision 659; ADR 0045; the Character sprites window's
`FieldSpriteCatalogViewModel` (records by image file, every image listed) as the model.

## Goal
The window's state: every image in `content/art/effects/` in name order, each with its record or the unlisted pace,
then the records whose image is missing (marked, still editable and saved, decision 442). Choosing one shows its boxes;
editing them lists it; **Reset to unlisted** puts the unlisted pace back, so OK removes its record. The swing box offers
the effects images. The problem line is a chosen strip's own problem, else Data's message for the whole list
(`CatalogProblems.OfEffectStrips`); OK greys while there is one. The whole session is one `CatalogDraft.SetEffectStrips`
step. Nothing names a record by id, so no undo is refused.

## New types
- `Editor/Battle/EffectStripCatalogViewModel` (internal sealed partial, `ObservableObject`): every effects image with its
  record or the unlisted pace, and the records whose image is missing. Constructor `(ContentCatalog current,
  Func<string, IReadOnlyList<string>> artImagesIn, Func<string, Point?> imageSizeOf)`; `Strips`
  (`EffectStripViewModel`s); `SelectedStrip`; `SwingChoices` (the effects images); `ResetToUnlistedCommand` (greyed
  while the chosen strip isn't listed); `Problem`; `CanApply`; `IReadOnlyList<EffectStripDefinition> ToStrips()` (the
  listed ones, sorted by file).
- `Editor/Battle/EffectStripEditingViewModel` (internal sealed): starts the window's session from the draft and applies
  it as one step. `Start()`, `Apply(EffectStripCatalogViewModel edited)` (as `DropTableEditingViewModel`).

## Changes
- `Shell/MainViewModel.Database.cs`: builds the editing view model there (the backlog's rule: not in the constructor's
  list, `MainViewModel.cs` being at 283).

## Tests
- `EffectStripCatalogViewModelTests` (new), images and records from `EffectStripFixtures` (EE2's):
  - `Strips_ImagesAndRecords_ListsEveryImageThenTheMissing` (an image without a record, one with, a record whose image
    is missing; set up in the opposite order).
  - `Strips_ImageWithoutARecord_ShowsTheUnlistedPace`.
  - `ToStrips_UntouchedImage_WritesNoRecord`; `ToStrips_EditedImage_WritesItsRecord`.
  - `ResetToUnlisted_ListedStrip_DropsItsRecord`; `ResetToUnlisted_UnlistedStrip_IsGreyed` (the command's `CanExecute`).
  - `Problem_ChosenStripsBoxNotANumber_IsItsProblem`; `Problem_ListFine_IsNone`; `CanApply_WithAProblem_IsFalse`.
  - `Problem_BoxChanged_IsRaised` (the relay from a strip's typed fields only).
- `EffectStripEditingViewModelTests` (new): `Apply_EditedSession_IsOneUndoStep`; `Apply_UntouchedSession_AddsNoStep`.

## Verify by hand
None: no window yet (EE4).

- **Done, with deviations:** the constructor takes the effects folder's readable images and a size lookup
  (`(int Width, int Height)?`, EE2's), not a folder lookup and a `Point`; the editing view model reads the folder when
  the window opens. `MainViewModel.cs` gains one constructor line (`EffectStripEditing = CreateEffectStripEditing()`,
  286 lines), as its get-only properties are set there; the factory is in `MainViewModel.Database`. A problem in a strip
  not chosen is named by its file. `EffectStripViewModel` gains `ResetToUnlisted()`. 99 + 20 lines, tests 210 + 51.
- **Review:** `SwingChoices` tested; a record already at the unlisted pace (Data takes one) stays listed while its
  boxes hold it, so an untouched OK changes nothing, and Reset to unlisted drops it (`EffectStripViewModel` remembers
  the record it was loaded from; 135 lines, tests 259). A second missing-image record tests their order.
