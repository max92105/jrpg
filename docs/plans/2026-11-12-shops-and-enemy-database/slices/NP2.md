# NP2. The dialogue step's portrait picked from the portraits folder

Data + Editor, about 120 lines (50 of them tests). Decision 589. Independent of phase 1.

## Goal
A dialogue step's **Portrait file** box becomes an editable list of the PNGs in `content/art/portraits/`, so a portrait is picked, not typed; a typed name still works (the startup check refuses a missing one, the editor never locks out, decision 442).

## New types
None.

## Changes
- `Data/Art/ArtFolders.ImagesIn(string artRoot, string folder)` (public): the PNG names of any art folder, sorted ordinally (the private `PngNamesIn` exposed through it; the two existing methods unchanged). Also used by EN6 and EC2.
- `Editor/Events/StepChoices`: `PortraitFiles` (init, empty by default, so dialogs built without it stay as they are).
- `Editor/Events/EventEditingViewModel`: fills `PortraitFiles` from a lookup it is given (`Func<IReadOnlyList<string>>`, built by the shell over `ArtFolders.ImagesIn(ArtRoot, ArtFolders.Portraits)`), read when a dialog opens.
- `Editor/Events/EventStepViewModel` (265): `PortraitFiles` (the choices' list), two lines.
- `Editor/Events/EventStepEditor.xaml`: the portrait `TextBox` becomes an editable `ComboBox` (`IsEditable`, `Text` bound to `PortraitFile`, the dark template, `BasedOn` the theme's style).

## Tests
- `ArtFoldersTests` (new or existing): `ImagesIn_FolderWithPngsAndOtherFiles_ListsPngsSorted`.
- `EventEditingViewModelTests`: `Start_PortraitLookup_OffersItsFiles`.

## Verify by hand
1. Open a Talk event's dialogue step: the Portrait box drops down the portraits folder's files; pick one, OK, Ctrl+S, play: the portrait shows beside the line.
2. Type a name that doesn't exist: the dialog accepts it; the game's start names the missing file. Revert.
