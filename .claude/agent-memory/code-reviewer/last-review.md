---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-26-stats-and-gear-slots, slice G1a-1 (items have a kind; armor becomes Chest). Round 1. Verdict: Ready to commit.

- Warning: PartyEquipment.TryEquip / PartyMember.SlotOf refusing a kind that fits no slot (new branch) has no test; AsWorn(Ring) makes it one line — open
- Suggestion: PartyEquipment.cs:44 `<param name="slot"></param>` left empty; delete it or say "the slot to list" — open
- Suggestion: EquipmentPageComparisonTests.cs:13 comment still says rows "Weapon, Armor" — open
- Suggestion: Validate_KindWithoutSlotYet_NamesTheKind sits between the private helpers and uses "items.json" instead of FileName/Assert.Equal like its neighbours — open
