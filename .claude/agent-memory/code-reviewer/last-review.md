---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-sprite-cells-and-mirrors, slice SC2 (RequiredStructMembers, FacingSequences null filter lines,
FieldSpriteCatalogValidator sequence checks, loader/writer/art tests). 2026-11-15. Round 1. Verdict: Ready to commit.

- Warning (open): FieldSpriteCatalogValidator.cs:50 comment "A JSON null in place of the list reads as null too" is
  false: fsi probe through ContentJsonOptions.Strict (RespectNullableAnnotations) throws on `{"cells": null}`; the
  null half of `is not { Count: > 0 }` guards an impossible state. Fix: drop the comment, test `Count == 0`.
- Warning (open): FieldSpriteCatalogValidator.cs:53 `facing.ToString().ToLowerInvariant()` duplicates
  ContentJsonOptions.EnumName (used by CraftingRulesValidator, LootRulesValidator).
- Fixed from SC1: SC1.md Done note width; ArtReferenceCheck `(long)` cast gone.
