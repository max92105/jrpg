---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU10 (ItemEquipmentViewModel: kind, weapon damage and damage type;
ItemViewModel.Equipment relayed; Equipment section on ItemCatalogPage.xaml). 2026-11-16. Round 1. Verdict: Ready to
commit (no Critical, no Warning).

- Suggestion (open): a catalog test that a ring turned weapon with damage typed but no type shows Data's
  "equipment.damageType is required for a weapon." (the reason the type box has no "none").
- Suggestion (open): ItemEquipmentViewModel.Problem summary says "null otherwise" though a weapon with a number is null too.
- Checked, no finding: Data requires a type on a weapon (ItemCatalogValidator:126); probe (PowerShell -STA) of
  weapon/consumable/ring switches: no write-back, null type shows blank, Kind and type shown right; no StackPanel theme
  style, so no BasedOn needed; untouched unique and weapon round-trip equal; new equipment starts Chest.
- IU9 Warning (event-step copies): fixed in part (CatalogUsage.MapsWithStep shared); refusal methods left, not carried.
