---
name: migration-roadmap
description: How the legacy WPF demo migration is split into plans (part 1 battle/party, part 2 world/save, part 3 game+editor+legacy removal), naming vocabulary, ADR/decision/question numbering
metadata:
  type: project
---

Part 1 = docs/plans/2026-10-17-legacy-core-migration.md (done; decisions 1-25, ADRs 0001-0004). Part 2 = docs/plans/2026-10-17-world-exploration-core.md (done; decisions 26-45; ADRs 0005-0007). Part 3 was split (decision 77, 2026-10-18). Part 3a = docs/plans/2026-10-18-shell-cutover-and-map-editing.md (titled "part 3a: game shell, content cutover and map editing", APPROVED): ADRs 0008-0012, slices 1a-1c, 2a-2c, 3a-3b, 10 cutover, 11a-11j editor. Parts 3b (game parity) and 3c (editor catalogs, legacy to legacy/) are unapproved notes at the end of that file; each needs its own /plan-feature. Autosave (Q44, Q67) and event battles (Q30) are new features, not parity. Decisions 46 FontStashSharp, 47 CommunityToolkit.Mvvm, 48 1920x1080/48 px fit+letterbox, 72 no editing gap, 73 legacy -> legacy/, 77 split + review fixes; Q78 window mode open.

Part 3a rules: both apps read the repo content/ in Debug via AssemblyMetadata("ContentRoot") + --content (Data ContentRootLocator), content copied only on publish; legacy art read from Assets/ (ArtRoot metadata) until slice 10.

**Why:** developer wants "nothing legacy left" before new features, and Play in Visual Studio launching game + editor.

**How to apply:** reuse vocabulary (PartyMember, Enemy, Combatant, *Definition, BattleState, PartyState, ContentCatalog, IRandom.NextBelow, ExplorationState, EventRun, WorldState, MapLayout/MapSettings/MapDefinition, SaveGame, FileSaveStore; part 3: SceneRequest/GameFlow, TargetChoice<T>, DrawOrderKey/MapLayers, MapDraft/IMapEdit/EditHistory, ContentWriter, ArtReferenceCheck, UiMessages). Next question/decision number 79, next ADR 0013. See [[legacy-content-quirks]], [[plan-lessons]].
