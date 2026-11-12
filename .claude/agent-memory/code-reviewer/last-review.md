---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-game-code-health-cleanup, slice GX1 (Game dead code with evidence; docs-only diff: Done note listing
78 members, all kept), 2026-11-12. Round 1. Verdict: ready to commit. No Critical.

- Verified: test seams Beats, CardHeight, IsChoosingCategory, OwnedColors, ShownGold, LevelNumber read by Game tests;
  all eight "own file, no test" members have real in-file readers; all 323 MessageIds/UiArtFiles/UiSoundFiles
  constants have a reader outside their files.
- Warning (open): the bare-name listing misses members sharing a name with another Game member (SelectionPosition x5,
  EventPresenter.ShownLine/Question, ForgeScene.SelectedPiece, QuestsPage/ShopScene.Category, SavePage.Notice/
  Entries/IsAsking, LoadPage.Entries, InventoryPage.IsDescriptionShown/ShownItemIds/Notice/SelectedRow, PartyPage.
  Notice/CardOf, LeaderChoiceScene.IsAsking, OptionsPage.SelectedRow, UiTheme.RarityMagic/Rare/Unique); none dead, but
  the note's 78/18 counts are incomplete. Fix: one sentence in the Done note.
- Follow-up: the eight in-file-only public constants/members could be private.
- GT3 Suggestion (Done note 128 vs 124 lines): not re-checked this round; GT3 committed.
