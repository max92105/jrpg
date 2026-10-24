---
name: validator-null-ids
description: Recurring in Data validators and the converter - null list elements or dictionary values pass strict STJ reading and crash later without a file name
metadata:
  type: feedback
---

System.Text.Json's `RespectNullableAnnotations` does not cover collection elements, so `["sarxian", null]` deserializes into an `IReadOnlyList<string>`. The common validator idiom `var unknown = ids.FirstOrDefault(id => !set.Contains(id)); return unknown is null ? null : ...` returns null both for "all known" and for "found a null id", so the null passes. `FrozenSet<string>.Contains(null)` returns false (no throw); `FrozenDictionary[null]` throws ArgumentNullException later in Core, without the file name.

Verified 2026-10-17 in slice 8 (NewGameValidator recruitedMemberIds: loader accepted it, `GameSession.StartNew` threw). Same idiom pre-exists in EncounterCatalogValidator (enemyIds) and PartyMemberCatalogValidator (skillIds).

Seen again in part 2 slice 1c (nested list: `Tiles: [null]` inside a source object -> NRE in ObjectConversion.TileGroupSize, while the outer list null was guarded).

Seen again in part 2 slice 1b (dictionary-value variant): the converter reads a dictionary of source tile records; `{ "1": null }` passes the strict reader and `TileConversion` throws a NullReferenceException that `Program` doesn't catch (verified with `dotnet fsi` + reflection on the internal type). Dictionary values are elements too.

**Why:** CLAUDE.md/code-quality rule: validate at boundaries with a message naming the bad field.

**How to apply:** In any validator over a list of ids, check for a null/blank element first. Probe with `dotnet fsi` + a scratch copy of `content/` (see [[scratch-builds-blocked]]).

Related strict-reading gap (part 2 slice 5b): `JsonNode.Parse` with default options accepts duplicate keys and `JsonObject` then throws ArgumentException (not JsonException) on first access, bypassing `AllowDuplicateProperties = false` in the serializer options. Any JsonNode pre-parse (save migrations) needs `JsonDocumentOptions { AllowDuplicateProperties = false }`.

Seen again in part 2 slice 5c (duplicate-entry variant): save lists aren't checked for duplicate ids. `Members: [sarxian, sarxian]` survives Read/repair, and `GameSession.ActiveOrder`'s `.Single` throws InvalidOperationException, outside the UnreadableSaveException contract. For any id-keyed list read from a file, ask: null entries, then duplicates.
