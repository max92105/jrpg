# PM2. One member as typed and the members as a catalog

Editor, about 220 lines (about 120 of them tests). Needs PM1, CL6 (`RecordChoice`). Design: sections 1, 3; decisions
953, 967; the mode bar's WS12 recipe, steps 1 and 9.

## Goal
The Party members page's state, headless: every member of the draft in id order, the added ones after; Add (the
blank, last, chosen) and Remove (only a member added this session); one member as typed: id (typed while new), name,
class chosen from the draft's classes by name, summary, and the portrait, battle sprite and attack effect file names as
typed text (the boxes offer the folders' files in PM4). The problem line is the chosen member's own (the id, "Class
must be chosen"), then any member's by id, then Data's message (`CatalogProblems.OfPartyMembers`). No Duplicate
(decision 953's default). The catalog view model is an `IEditedCatalog` (the chosen member's id).

## New types
- `Party/PartyMemberViewModel` (`ObservableObject`): one member as the Party members page types it.
  - `From(PartyMemberDefinition)`, `New()` (the blank, design section 1), `IsNew`.
  - `[ObservableProperty]` `Id`, `Name`, `ClassId` (the combo's `SelectedValue`), `Summary`, `PortraitFile`,
    `BattleSpriteFile`, `AttackEffectFile`.
  - `Label` ("omer: Omer", "(new member)"); `Problem`: the id rule while new (`ObjectIds.ProblemOf` against the other
    members' ids), then "Class must be chosen." while `ClassId` is empty.
  - `PartyMemberDefinition ToMember()`: a new record from the boxes, names trimmed (every field of the record is a box,
    so nothing is carried); throws on a problem.
- `Party/PartyMemberCatalogViewModel` (`ObservableObject`, `IEditedCatalog`): every member on the page, Add, Remove
  while new, the class choices and the problem line.
  - `PartyMemberCatalogViewModel(ContentCatalog content)`; `Members`; `SelectedMember`;
    `IReadOnlyList<RecordChoice> Classes` (the draft's classes by name, `Catalogs/RecordChoice` from CL6).
  - `ChosenKey` = `SelectedMember?.Id`; `Choose(key)`, an unknown key keeping the choice.
  - `Problem` (raises its change), `CanApply`, `ToMembers()`; `AddMember`, `RemoveMember` (greyed unless `IsNew`); the
    typed-field relay as `ClassCatalogViewModel`'s.

## Tests
- `Party/PartyMemberFixtures` (new, shared by this slice's and PM3's test files): `Scholar`, a member built in the file
  naming `ClassFixtures.Scholar`'s class, every art name distinct.
- `Party/PartyMemberViewModelTests` (new):
  - `From_Member_ShowsEveryField`.
  - `ToMember_TypedSummary_SetsIt`; `ToMember_TypedNameWithSpaces_TrimsIt`.
  - `ToMember_TypedArtNames_SetsAllThree` (each name distinct, so a swapped pair fails).
  - `Problem_NoClass_SaysClassMustBeChosen`; `Problem_NewMemberWithBadId_NamesTheId`;
    `Problem_SavedMember_IgnoresIdRule`.
- `Party/PartyMemberCatalogViewModelTests` (new; a draft over the test world `with` `Classes` and `PartyMembers` from
  the fixtures and a second member built in the test):
  - `Constructor_Members_ListsThemInIdOrder` (set up in the opposite order).
  - `Classes_ListsTheDraftsClassesByName`.
  - `AddMember_AddsBlankLastAndChoosesIt`; `RemoveMember_SavedMember_IsGreyed`; `RemoveMember_AddedMember_RemovesIt`.
  - `Problem_EmptySummary_ShowsDataMessage`; `Problem_TypedChange_RaisesProblemChanged`.
  - The recipe's three: `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`,
    `ChosenKey_NothingChosen_IsNull`.

## Verify by hand
None: the page comes in PM4.
