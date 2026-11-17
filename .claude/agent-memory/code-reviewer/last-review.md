---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU14 (Editor/Loot/AffixViewModel, EquipmentKindToggle; tests
AffixFixtures, AffixViewModelTests, AffixViewModelNoticeTests). 2026-11-16. Round 1. Verdict: Ready to commit
(no Critical, no Warning).

- Suggestion (open): private `TypedRecord()` returns the source with the typed id, stat and kinds but the source's
  numbers; a name like `RecordWithTypedChoices` would say so without the comment.
- Suggestion (open): `AffixFixtures.Catalog` (static auto-property) loads test-world in the class's type initializer,
  so every test reading `Fierce` loads it, and a load failure or id clash fails them all; make it lazy or move it to
  IU15's catalog tests.
- Follow-up (open): `AffixDefinition` compares `Kinds` by reference while `ItemDefinition` overrides Equals with
  SequenceEqual; a Core Equals (with its test) would retire `AssertSameContent` and IU15's content helper.
- Checked, no finding: Loot/ folder matches Core.Loot and the map row; Duplicate's copy subscribes its own toggles,
  copies half-typed boxes (no parse); Changed raised from the partials, never from notices; New matches design.md
  tunables (power, 1..1, 10, 20, no kind); ObjectIds.ProblemOf(Id, []) as every sibling page.
- IU13 Suggestions: not carried (round closed).
