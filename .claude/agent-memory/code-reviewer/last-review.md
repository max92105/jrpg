---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice NG1 (new-game.json joins the catalogs' write and stamps). 2026-11-17.
Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: no Save all (WriteMapCatalogs) test with a new-game sprite lacking a record; only WriteMaps and
  OfNewGame pin that half of decision 1018.
- Suggestion, open: ContentWriter.CheckMaps summary and CheckedMapWrites comment still say "the new game's start"; the
  check is now the whole new game, sprite record included.
- Follow-up, open: CheckItems summary (CatalogProblems.Items.cs:29-31) says "starting items"; decision 1018 makes its
  whole-new-game validator call load-bearing for Save all.
- Follow-up, open: RenameMap writes new-game.json with no IsEditedOnDisk check (pre-existing), now renewing the stamp.
- Follow-up, open: NewGameReferences + NewGameValidator.Validate in the loader, CheckItems and CheckNewGame.
- PM4b follow-ups (StripFromCellsWindow catch, TileImages "found readable"): dropped, backlog.md lists both.
