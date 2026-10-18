# 0007. Save format and versioning

Status: accepted
Date: 2026-10-17

## Context
Slices 5a–5c add save and load: party progress, worn equipment, inventory, the player's location, the world's changes (done "once" events, swapped objects) and play time. The game will have 20 manual slots plus an autosave (decision 34); part 2's store just takes a slot name. Legacy save files are refused with a clear error (decision 20), and the random state isn't saved (decision 43).

Two choices are made once here: what shape a save has, and how it survives format changes. Constraints: Core stays free of I/O and serializer attributes (ADR 0002); runtime classes (`PartyMember`, `Inventory`, `WorldState`) are never serialized directly (ADR 0002 consequence); CLAUDE.md requires a round-trip test, and a migration plus a test loading the old format whenever the format changes; content changes between versions must not brick a save, it drops what no longer exists (decision 40). Solo developer: the format will change often while the game is young.

## Options
### A. Immutable snapshot records in Core, a `formatVersion` envelope in Data
`GameSession.Capture(savedAt)` builds a `SaveGame` record (members as `PartyMemberProgress`, ids everywhere, no object references); `GameSession.Restore(save, catalog, random)` rebuilds the session. Data writes `{ "formatVersion": N, "game": { … } }` and, on read, upgrades older files step by step on the raw JSON (`JsonNode`) before deserializing.
- Pros: one save shape, readable in tests without files; Core decides what a session is, Data only how it's stored. Migrations are small functions on JSON, each with an old-format test file. Same strict reading as content.
- Cons: a renamed `SaveGame` property is a format change (needs a version bump and a migration). Migrations on `JsonNode` are untyped.

### B. Save DTOs in Data, mapped from the runtime classes
Data reads the session's state and maps it to its own `*Json` types.
- Pros: Core's types can change freely; the file format only changes when the DTOs do.
- Cons: Data must reach into runtime state Core keeps private, or Core exposes it anyway; two parallel types per concept plus mappers both ways; the same drift ADR 0002 avoided for content.

### C. Snapshot records with no version
As A, but without an envelope: a format change makes old saves unreadable.
- Pros: least code now.
- Cons: every format change breaks every save, which CLAUDE.md forbids; adding a version later means guessing which unversioned files are which.

## Decision
**A** (developer, 2026-10-17), with these rules:
- A format change bumps `formatVersion` and ships a migration from the previous version, with a test loading a file in the old format.
- Writes replace the file atomically (write a temporary file, then move it over), so a crash mid-save can't destroy the previous save.
- On read, structural problems make the save unreadable with a reason: bad JSON, missing fields, an unknown or missing version, a legacy file. Content mismatches are repaired, not refused: unknown members, items, maps and events are dropped and the rest kept (decision 40).
- Only what play changes is saved. Values derived from content (max HP/MP, stats) are recomputed on load until stat growth exists.
- A save is only captured between events and battles, so `EventRun` and a pending encounter are never saved.

## Consequences
- Renaming or removing a `SaveGame` field is a deliberate format change, caught by the round-trip and old-format tests.
- Repair needs the loaded content: a read save goes through a repair step against the `ContentCatalog` before `GameSession.Restore` (slice 5c), which lists what it changed so the game can tell the player (part 3).
- The autosave slot question (decision 34, question 44) doesn't affect the format; slots are just names.
