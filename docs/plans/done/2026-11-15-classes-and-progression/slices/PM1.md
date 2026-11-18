# PM1. `party-members.json` is written by the editor

Data + content, about 130 lines (about 80 of them tests) plus the relaid `content/party-members.json`. Design: sections
3 (Data changes), 6; decision 952.

## Goal
Save all writes `party-members.json` like every other catalog: sorted by id, checked first against the draft's classes
as the loader checks it, only when it changed. The repository's file is already one field per line; the relay only
sorts it by id. Nothing edits members yet (PM2).

## Changes
- `Data/Content/ContentCatalogLoader.cs`: `PartyMembersFile` becomes `internal`.
- `Data/Content/CatalogProblems.Party.cs` (CL1's partial): `public static string? OfPartyMembers(IReadOnlyList
  <PartyMemberDefinition> members, ContentCatalog content)` (a member's own rule or a class the content lacks) and
  `internal static void CheckPartyMembers(...)`: `PartyMemberCatalogValidator.Validate(PartyMembersFile, members, new
  PartyMemberReferences(content.Classes.Keys.ToHashSet()))`.
- `Data/Content/ContentWriter.Catalogs.cs`: `CheckPartyMembers` after the classes' check in `CheckCatalogs` (a member
  names a class); `party-members.json` in `SerializedCatalogs`; `SortedPartyMembers`.
- `content/party-members.json`: relaid once by the writer (a throwaway test, deleted after; the diff only reorders).
- `docs/map.md`, Data `Content` row: `CatalogProblems.OfPartyMembers`; the writer's files gain `party-members.json`.

## Tests
- `Content/ContentWriterPartyMemberTests` (new; test-world copy; members built in the test with `with` over a member
  made there):
  - `WriteMapCatalogs_PartyMembers_LoadTheSameAfterRelay`.
  - `WriteMapCatalogs_NewMembers_WritesThemSortedById` (`zzz` added to the catalog before `aaa`: the file holds `aaa`
    first and `zzz` last, so the sort, not the insertion order, places them).
  - `WriteMapCatalogs_MemberNamingUnknownClass_WritesNothing` (the loader's message, the file's bytes unchanged).
  - `WriteMapCatalogs_ClassRemovedWhileAMemberNamesIt_WritesNothing` (the classes without the member's class).
- `Content/CatalogProblemsPartyTests` (CL1's file): `OfPartyMembers_ValidMembers_IsNull`;
  `OfPartyMembers_EmptySummary_NamesTheMember`.
- `Content/ContentWriterRepositoryTests`: `CatalogFileBytes` gains `party-members.json` (after the relay).

## Verify by hand
1. `git diff content/party-members.json` in the relay commit: the same nine members, in id order.
2. Run the game: the leader choice lists the nine characters in `new-game.json`'s order, as before.

- **Done, with deviations:** `party-members.json` joins the writer's catalogs and stamps, checked after the classes;
  relaid once (83 lines and 3015 bytes before and after, the nine members in id order, each record unchanged; the
  simulations' lines identical). One test beyond the list, `OfPartyMembers_ClassTheContentLacks_NamesIt`. The removed
  class test saves a member of a test class, then saves without the class, so no repository member is picked. Sizes:
  Data `CatalogProblems.Party.cs` 70, `ContentWriter.Catalogs.cs` 151; tests `ContentWriterPartyMemberTests` 108,
  `CatalogProblemsPartyTests` 85, `ContentWriterRepositoryTests` 90 (about 30 lines of Data, 145 of tests).
