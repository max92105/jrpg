---
name: migration-roadmap
description: How the legacy WPF demo migration is split into plans (part 1 battle/party, part 2 world/save, part 3 rendering, part 4 editor), naming vocabulary, ADR/decision/question numbering, temporary source-of-truth exception
metadata:
  type: project
---

Part 1 = docs/plans/2026-10-17-legacy-core-migration.md (done; decisions 1-25, ADRs 0001-0004). Part 2 = docs/plans/2026-10-17-world-exploration-core.md (approved 2026-10-17; 16 slices; decisions 26-43; open questions 30, 44, 45; ADRs 0005-0007). Part 3 = MonoGame rendering, and also receives from part 2: cutscenes (design ready), event-started battles (StartBattleStep, Q30), render-only content (tilesheets, layer Z, object tiles, AbovePlayerZones), SaveSlotSummary, defeat flow, stick deadzone, autosave mapping. Part 4 = editor port; ends the converter.

Temporary exception (decision 42): developer keeps editing maps in the legacy MapEditor, so `Assets/` is the source of map geometry until part 4; `tools/Jrpg.ContentConverter` rewrites only `content/generated/`; hand-written map settings live in `content/maps/{id}.json`.

**Why:** legacy logic is ~2x what a plan of ~300-line slices holds; the developer has no new editor until part 4.

**How to apply:** reuse vocabulary (PartyMember, Enemy, Combatant, *Definition, BattleState, PartyState, ContentCatalog, IRandom.NextBelow; part 2: ExplorationState, EventRun, WorldState, MapLayout/MapSettings/MapDefinition, MapLocation in Maps/, ExplorationRules in Content/, PartyMemberProgress in Party/, Speech, SaveGame, FileSaveStore). Next decision/question number 46, next ADR 0008. See [[legacy-content-quirks]], [[plan-lessons]].
