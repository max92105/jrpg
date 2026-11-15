# PM3. The members' undo step, refused while a map recruits or requires an added member

Editor, about 170 lines (about 90 of them tests). Needs PM2. Design: section 3; decision 954; the mode bar's WS12
recipe, step 2.

## Goal
Applying the Party members page records one `CatalogDraft.SetPartyMembers` step (nothing when the same by content),
through `PartyMemberEditingViewModel`, an `ICatalogEditing<PartyMemberCatalogViewModel>`. An undo that would remove a
member a map's event recruits (`RecruitStep`) or gates on (`MapEvent.OnlyWhenLeaderId`) is refused with "Can't undo:
party member ranger is used on map forest.", the open maps' unsaved events included, as the shop and cinematic undos
are refused (decision 581). The new game names members too, but its step always comes after the member's, so catalog
order covers it (decision 954).

## New types
- `Party/PartyMemberEditingViewModel(CatalogDraft catalog)` (`ICatalogEditing<PartyMemberCatalogViewModel>`): starts
  the Party members page from the draft and applies its session as one step. `Start()`, `Apply(edited)`. (PM4 gives it
  the art folder lookup.)

## Changes
- `Catalogs/CatalogDraft.Party.cs` (CL3's partial): `SetPartyMembers(members)` (records compare by value: no list
  fields) and `PartyMembersRemovedByUndo()` over `KeysRemovedByUndo`.
- `Catalogs/CatalogUsage.cs` (149 lines, about 165 after): `UsersOfPartyMember(ContentCatalog catalog, string memberId,
  IReadOnlyCollection<MapLayout> openLayouts)`: the maps whose events recruit the member or name it as their leader
  gate, "map forest" style, sorted, through `MapNamesWhere`.
- `Shell/CombinedHistory.cs` (221 lines plus row 11's WS4, about 245 after): `RefusalOfPartyMemberUndo()`, beside
  `RefusalOfShopUndo`, joined into `RefusalOfCatalogUndo`'s first line; the method's comment gains the member.

## Tests
- `Catalogs/CatalogDraftPartyMemberTests` (new; members from `PartyMemberFixtures`):
  `SetPartyMembers_ChangedSummary_RecordsOneStep`; `SetPartyMembers_SameMembers_RecordsNothing`;
  `SetPartyMembers_Undo_RestoresTheMembers`; `PartyMembersRemovedByUndo_AfterAdding_NamesTheAddedMember`.
- `Catalogs/CatalogUsagePartyMemberTests` (new; layouts built in the test):
  `UsersOfPartyMember_RecruitStep_NamesTheMap`; `UsersOfPartyMember_LeaderGate_NamesTheMap`;
  `UsersOfPartyMember_OtherMember_IsEmpty`; `UsersOfPartyMember_OpenMapUnsavedStep_NamesIt`.
- `Shell/CombinedHistoryPartyMemberTests` (new; as `CombinedHistoryShopTests`):
  `Undo_AddedMemberRecruitedOnOpenMap_IsRefusedNamingTheMap`; `Undo_AddedMemberUnused_RemovesIt`.
- `Party/PartyMemberEditingViewModelTests` (new): `Apply_EditedName_ChangesTheDraft`.

## Verify by hand
None: the page comes in PM4, whose hand check covers the refusal.
