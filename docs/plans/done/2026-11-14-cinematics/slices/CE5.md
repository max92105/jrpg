# CE5. The Cinematics window's rules

Editor + Data, about 220 lines (about 120 of them tests). Needs CE2, CE4. Design: "Structure" (Editor), guiding rule
"The editor never locks out on missing art or sound".

## Goal
The window's state headless: every cinematic in id order then the new ones, Add (a typed id), Remove while new, the
pictures, voices and tracks the boxes offer (with a mark for a name the folders lack), the problem line (the chosen
cinematic's, then Data's for the whole list), and OK applying the session as one `SetCinematics` step.

## New types
- `Editor/Cinematics/CinematicMediaChoices` (internal sealed): the pictures, voices and tracks the boxes offer, read
  through lookups when the window opens. `Pictures` (every PNG under the cinematics folder, subfolders included, sorted),
  `Voices` (`AudioFolders.FilesIn(audioRoot, Voices)`), `Tracks` ("" for none, then `AudioFolders.TracksIn`);
  `MissingOf(CinematicViewModel)` (the names its stills or track give that the lists lack, for the window's note).
  Takes `Func<IReadOnlyList<string>>` lookups, never reads files itself (the view-model rule).
- `Editor/Cinematics/CinematicCatalogViewModel` (internal sealed partial): every cinematic as the window edits it, and
  the window's problem line. `Cinematics` (id order, new ones after), `SelectedCinematic`, `NewId` box and `Add`,
  `Remove` (only `IsNew`), `Choices`, `Problem` (the chosen cinematic's, then `CatalogProblems.OfCinematics` over every
  cinematic's `ToCinematic()` when none has its own, through `Catalogs/RecordListProblem` for an id listed twice),
  `CanApply`, `ToCinematics()`. Relays each cinematic's `StillsChanged` and id typing to `Problem`, never `Label` or
  `Problem` notices.
- `Editor/Cinematics/CinematicEditingViewModel` (internal sealed): starts the window's state from the draft and applies
  it as one undo step. `Start()` → `CinematicCatalogViewModel`; `Apply(CinematicCatalogViewModel)` →
  `CatalogDraft.SetCinematics`.

## Changes
- Data `Art/ArtFolders`: `ImagesUnder(string artRoot, string folder)`: the PNG names in a folder and its subfolders by
  exact name with `/` (`ExactFileName.NameUnder`), sorted ordinally; empty when the folder is missing. Summary: "the
  cinematics' pictures, one folder per cinematic (ADR 0046)".

## Tests
- `ArtFoldersTests`: `ImagesUnder_PictureInSubfolder_NamesItWithItsFolder`; `ImagesUnder_NoFolder_IsEmpty`;
  `ImagesUnder_NotPng_IsLeftOut`.
- `CinematicMediaChoicesTests` (new): `Tracks_FirstEntry_IsNone`; `MissingOf_StillPictureNotInFolder_NamesIt`;
  `MissingOf_EverythingThere_IsEmpty`; `MissingOf_VoiceNotInFolder_NamesIt`.
- `CinematicCatalogViewModelTests` (new), a draft over the test world `with` two cinematics built in the test:
  - `Cinematics_SavedThenNew_InIdOrderThenAdded`.
  - `Add_TypedId_AddsAndChoosesIt`; `Add_IdTaken_ShowsProblemAndAddsNothing`.
  - `Remove_Saved_IsRefused` (greyed: `CanExecute` false); `Remove_New_RemovesIt`.
  - `Problem_StillZoomPast150_IsDatasMessage` (the row parses, Data refuses).
  - `CanApply_ProblemShown_IsFalse`; `PropertyChanged_CanApply_RaisedWhenATypedSecondsChanges`.
- `CinematicEditingViewModelTests` (new): `Apply_AddedCinematic_IsOneUndoStep`; `Apply_NothingChanged_RecordsNothing`.

## Verify by hand
None: the window comes in CE6.

- **Done, with deviations:** the choices take one lookup each (pictures, voices, tracks) and the editing view model a `Func<CinematicMediaChoices>`, read when the window opens. The catalog view model also gives `MissingNote` ("Not in the folders: ...") for the window's note, and `NewIdProblem` for the id box. No `ImagesUnder_NotPng_IsLeftOut`: the subfolder test holds a text file. Tests added: `MissingOf_TrackNotInFolder_NamesIt`, `Problem_RowCantBeRead_NamesTheCinematic`, `Problem_NothingWrong_IsNull`, the track's `CanApply` notice, the two `MissingNote` tests.
