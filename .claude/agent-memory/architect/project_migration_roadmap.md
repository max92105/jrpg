---
name: migration-roadmap
description: How the legacy WPF demo migration is split into plans (part 1 battle/party, part 2 world/save, part 3 game+editor+legacy removal), naming vocabulary, ADR/decision/question numbering
metadata:
  type: project
---

Part 1 = docs/plans/2026-10-17-legacy-core-migration.md (done; decisions 1-25, ADRs 0001-0004). Part 2 = docs/plans/2026-10-17-world-exploration-core.md (done; decisions 26-45; ADRs 0005-0007). Part 3 was split (decision 77, 2026-10-18). Part 3a = docs/plans/2026-10-18-shell-cutover-and-map-editing.md (done): ADRs 0008-0012, slices 1a-11j. Its end holds unapproved notes for 3b (game parity) and 3c (editor catalogs; 13a legacy->legacy/ already done, Assets/ deleted). Decisions 46 FontStashSharp, 47 CommunityToolkit.Mvvm, 48 1920x1080/48 px fit+letterbox, 72, 73, 77.

First 3b plan (revised 2026-10-18, proposed): docs/plans/2026-10-18-game-title-menu-movement.md — 26 slices: M1-M4 (M3a/b/c), W1 full screen, T1a/T1b/T2 title, G1-G4 menu shell/confirm/header/status, E1 editor uid highest+1, S1/S2a/S2b/S3 save-load, P1-P5 inventory/item use/equipment/party/options, Z check. Decisions 50-87 answered there (encounters off via Flow/EncounterFreeContent, borderless full screen, port legacy Theme.xaml look, always confirm Title/Quit). Open: 52, 60, 61, font, 88-93. Left for later 3b plans: battles (4a-4e), defeat (6d), cutscenes (7a-7c), prompt bubble, map banner, equipment "take from wearer", party sidebar.

**Why:** developer wants "nothing legacy left" before new features; asked (2026-10-18) for game first: title, game menu, movement.

**How to apply:** reuse vocabulary (PartyMember, Combatant, *Definition, BattleState, PartyState, ContentCatalog, ExplorationState, EventRun, GameSession, FileSaveStore; Game: SceneRequest records in Scenes/, GameFlow + SessionTransitions/MenuTransitions, SceneFactory, EventPresenter, ScreenFade, ScreenText, MapArt, SaveSlots/SaveSlotList in Saving/, IMenuPage). Next question/decision number 94, next ADR 0013. See [[legacy-content-quirks]], [[plan-lessons]].
