---
name: validator-null-ids
description: Recurring in Data validators - `ids.FirstOrDefault(id => !known.Contains(id))` silently passes a JSON null inside a string id list, which then crashes Core lookups
metadata:
  type: feedback
---

System.Text.Json's `RespectNullableAnnotations` does not cover collection elements, so `["sarxian", null]` deserializes into an `IReadOnlyList<string>`. The common validator idiom `var unknown = ids.FirstOrDefault(id => !set.Contains(id)); return unknown is null ? null : ...` returns null both for "all known" and for "found a null id", so the null passes. `FrozenSet<string>.Contains(null)` returns false (no throw); `FrozenDictionary[null]` throws ArgumentNullException later in Core, without the file name.

Verified 2026-10-17 in slice 8 (NewGameValidator recruitedMemberIds: loader accepted it, `GameSession.StartNew` threw). Same idiom pre-exists in EncounterCatalogValidator (enemyIds) and PartyMemberCatalogValidator (skillIds).

**Why:** CLAUDE.md/code-quality rule: validate at boundaries with a message naming the bad field.

**How to apply:** In any validator over a list of ids, check for a null/blank element first (the item validator already does `Any(string.IsNullOrWhiteSpace)` for allowedClassIds). Probe with `dotnet fsi` + a scratch copy of `content/` (see [[scratch-builds-blocked]]).
