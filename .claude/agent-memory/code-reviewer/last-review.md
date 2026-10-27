---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-26-stats-and-gear-slots, slice G1a-2 (members wear nine slots; TryEquip names the slot). Round 1. Verdict: Ready to commit.

- Warning: no test that two worn items outside Weapon/Chest sum (same ring twice stacks its bonus, decision 329); TryEquip_SameRingTwice_WearsBoth asserts placement only — open
- Suggestion: PartyEquipment.cs TryEquip(member,item,slot) returns doc "isn't equipment its kind fits the slot" -> "isn't equipment whose kind fits the slot" — open
- Follow-up: PartyEquipment.OwnedFor `<param name="slot"></param>` still empty (from G1a-1 review) — open
