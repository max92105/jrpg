---
name: migration-roadmap
description: How the legacy WPF demo migration is split into plans (part 1 battle/party, part 2 world/save, part 3 game+editor+legacy removal), naming vocabulary, ADR/decision/question numbering
metadata:
  type: project
---

Part 1 = docs/plans/done/2026-10-17-legacy-core-migration.md (done; decisions 1-25, ADRs 0001-0004). Part 2 = docs/plans/done/2026-10-17-world-exploration-core.md (done; decisions 26-45; ADRs 0005-0007). Part 3 was split (decision 77, 2026-10-18). Part 3a = docs/plans/done/2026-10-18-shell-cutover-and-map-editing.md (done): ADRs 0008-0012, slices 1a-11j. Its end holds unapproved notes for 3b (game parity) and 3c (editor catalogs; 13a legacy->legacy/ already done, Assets/ deleted). Decisions 46 FontStashSharp, 47 CommunityToolkit.Mvvm, 48 1920x1080/48 px fit+letterbox, 72, 73, 77.

First 3b plan: docs/plans/2026-10-18-game-title-menu-movement.md (approved, all slices done by 2026-10-20 except Z hand check). Decisions 50-87 there.

Second 3b plan (2026-10-20, approved after devil's advocate): docs/plans/2026-10-20-battle/README.md — ADR 0013 (HUD reads live state, battle stacked over map, event-driven beats) + 25 slices; Core changes sit with their first user (C1 session, C2 attack targets, C3 skill descriptions, C4 nobody-standing = defeat). Decisions 94-105 answered (gold/drops and battle sprite sheets = future plans with ADRs); open: serif font, 106 placeholder cue. Still left for later 3b plans: cutscenes (7a-7c), prompt bubble, map banner, event battles (q30).

**Why:** developer wants "nothing legacy left" before new features; battle was "the last big gap" (2026-10-20), asked "ready to grow and change".

**How to apply:** reuse vocabulary (PartyMember, Combatant, *Definition, BattleState, PartyState, ContentCatalog, ExplorationState, EventRun, GameSession, FileSaveStore; Game: SceneRequest records in Scenes/, GameFlow + SessionTransitions/MenuTransitions/BattleTransitions, SceneFactory, EventPresenter, ScreenFade, BattleScene, BattleTurnSequence, CommandMenu, ActionPlayback). Next question/decision number 107, next ADR 0014. See [[legacy-content-quirks]], [[plan-lessons]].
