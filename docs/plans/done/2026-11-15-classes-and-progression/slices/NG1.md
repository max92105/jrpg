# NG1. `new-game.json` joins the catalogs' write and stamps

Data, about 150 lines (about 100 of them tests). Needs PM1. Design: section 3 (Data changes); decisions 961, 969.

The coordinator asked for two new-game slices; at this plan's sizes the file, the view model with its step, and the
page are three, as every other catalog here is split.

## Goal
`new-game.json` is saved like the catalogs: checked first as the loader checks it (its own rules, its members, items,
map and cell, and a field sprite with a record), written by Save all only when the draft's new game differs from the
bytes stamped at load, refused when edited on disk since (decision 559). The map rename keeps writing it through the
same serialized bytes and renews its stamp, so a rename followed by a new game edit never reads as a hand edit. The
repository's file is already in the writer's layout (`NewGame_RepositoryFile_WritesBackUnchanged`): no relay. Nothing
edits the new game yet (NG2).

## Changes
- `Data/Content/CatalogProblems.Party.cs`: `public static string? OfNewGame(NewGameDefinition newGame, ContentCatalog
  content)` and `internal static void CheckNewGame(NewGameDefinition, ContentCatalog)`: `NewGameValidator.Validate
  (NewGameFile, newGame, new NewGameReferences(members, items, maps))`, then `FieldSpriteReferenceCheck.Check(...)` with
  the content's field sprites and objects, as the loader runs them.
- `Data/Content/ContentWriter.Catalogs.cs`: `new-game.json` in `SerializedCatalogs` (one object, no sort);
  `CheckNewGame` in `CheckCatalogs`. `ContentWriter.CheckSettingsAndNewGame`'s new game half calls the same method, so
  the two never drift.
- `Data/Content/ContentWriter.RenameMap`: the new game's write takes its bytes from the same serialization
  (`PlanWrite(CatalogWrite)`, one write path), and after `WriteAll` the stamp of `new-game.json` is renewed with them.
- `docs/map.md`, Data `Content` row: `new-game.json` written with the catalogs and stamped; `CatalogProblems.OfNewGame`.

## Tests
- `Content/ContentWriterNewGameTests` (new; test-world copy; the new game `with` changes built in the test):
  - `WriteMapCatalogs_ChangedGold_WritesNewGame` (read back equal).
  - `WriteMapCatalogs_UnchangedNewGame_LeavesTheFile` (stamped, then only a shop changed: the file's bytes unchanged
    and not rewritten).
  - `WriteMapCatalogs_LeaderNamingUnknownMember_WritesNothing`.
  - `WriteMapCatalogs_NewGameEditedOnDisk_Refuses` (stamped, the file edited on disk, the draft's gold changed).
- `Content/ContentWriterRenameTests`: `RenameMap_StartMap_RenewsTheNewGameStamp` (rename the start map, then change the
  gold and write: no "changed on disk" refusal).
- `Content/CatalogProblemsPartyTests`: `OfNewGame_ValidNewGame_IsNull`; `OfNewGame_SpriteWithoutRecord_NamesIt`.

## Verify by hand
1. Run the editor, change a map, Save all: `git status` shows no change to `new-game.json`.
2. Map > Rename map... on the start map, then rename it back: no refusal at the next Save all, and `new-game.json`
   names the map as at the start.
- **Done, with deviations:** `new-game.json` joins the writer's catalogs and stamps (`NewGameWrite`, shared with the
  map rename, which renews its stamp); no relay, the simulations' lines identical. Decision 1018: `CheckCatalogs`
  gains no `CheckNewGame` call, since `CheckItems` already runs the new game's validator and `CheckFieldSprites` its
  sprite's record (a third call checks it twice, no test can tell); `CheckNewGame` serves `OfNewGame` and
  `CheckSettingsAndNewGame`, so a map save now refuses a new game sprite without a record, as the loader does
  (`WriteMaps_NewGameSpriteWithoutRecord_WritesNothing`). Also beyond the list:
  `OfNewGame_LeaderTheContentLacks_NamesIt` (the validator's half),
  `WriteMapCatalogs_NewGameSpriteWithoutRecord_WritesNothing` (review: the sprite's half of decision 1018 on Save
  all), and `ContentWriterRepositoryTests.CatalogFileBytes` gains `new-game.json`. Review: `CheckItems`' summary
  names the whole new game check and decision 1018; the map checks' summaries say "the new game", not its start; two
  backlog lines (the rename's unchecked hand edit, the new game check's three copies). Sizes: Data
  `CatalogProblems.Party.cs` 95, `ContentWriter.Catalogs.cs` 158, `ContentWriter.cs` 237; tests
  `ContentWriterNewGameTests` 115, `CatalogProblemsPartyTests` 107, `ContentWriterRenameTests` 159 (about 40 lines of
  Data, 165 of tests).
