---
name: migration-roadmap
description: How the legacy WPF demo migration is split into plans (part 1 battle/party, part 2 world/save, part 3 rendering, part 4 editor) and the naming decisions made in part 1
metadata:
  type: project
---

Legacy migration is split: part 1 = docs/plans/2026-10-17-legacy-core-migration.md (skeleton, progression, items/inventory, party/equipment, battle, rewards+simulation, GameSession). Part 2 = maps/exploration/events/world state/save/cutscenes. Part 3 = MonoGame rendering. Part 4 = map editor port.

**Why:** legacy logic is about 2x what 8 slices of ~300 lines can hold, so part 1 stops at a headless playable battle built from content.

**How to apply:** when planning part 2+, reuse the part 1 vocabulary: PartyMember (not Character), Enemy, Combatant, Experience; Definition records for content; BattleState (runtime battle) with the BattlePhase enum; PartyState in Party/; Inventory in Items/; ContentCatalog plus ContentCatalogLoader; IRandom/SeededRandom. Check the plan file for the status of open questions (Q1 packages, Q2 Data.Tests, Q3 id scheme) before assuming. See [[legacy-content-quirks]] and [[type-folder-name-clash]].
