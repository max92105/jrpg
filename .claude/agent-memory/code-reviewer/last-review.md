---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU2 (ItemDefinition.IsUnique/FixedAffixes + custom equality;
GearPiece.Unique/Unrolled; Inventory.Add, GearRoll.Roll, ContentCatalog.PieceFrom; IU3's two filter rows pulled forward).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): ItemDefinition's custom Equals lists 12 members in two tuples + FixedAffixes; a member
  added later and left out passes every test. A reflection test (each init property changed alone makes items unequal)
  would guard the draft's change detection. Same gap on the other 9 hand-written record Equals in Core.
- Suggestion (open, optional): RolledAffix `AffixId` param doc "or a unique names" reads as missing a word.
- Checked, no finding: all 13 ItemDefinition members compared; hash consistent; unique consumable never reaches
  Unrolled (Inventory.Add branches on Equipment; GearRoll -> Rolled throws "not equipment"; IU3 refuses it at load);
  "no longer unique" saved piece is IU4's repair (decision 868), PieceFrom takes a repaired save; only Inventory.Add
  used Plain in src.
- IU1's open Suggestion (CheckItems shop/table order) dropped: optional, other slice.
