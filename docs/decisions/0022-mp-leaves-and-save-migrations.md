# 0022. MP leaves the game; no save migrations before release

Status: accepted
Date: 2026-10-24

## Context
Decision 197 removes MP. Every class has its own resource instead (ADR 0019), so MP leaves members, enemies, skills, effects, screens and saves. `EffectKind.RestoreMp` goes too: no content uses it, so no item retires.

Saves hold each member's `mp` (`PartyMemberProgress`), so this is the first change to the save format since ADR 0007. ADR 0007 asks every format change for a version bump, a migration from the previous version, and a test loading a file in the old format. LR2 changes the format again, for lasting resources.

Constraints:
- **Strict reading:** saves are read as strictly as content. A missing or unknown field makes a save unreadable (`SaveJsonOptions.Strict`).
- **Test saves only:** no player has a save yet. Every save on disk is the developer's own test save.
- **One developer:** the format will change often while the game is young.

## Options
### A. An ordered chain of JSON migrations in Data
`Saving/SaveMigrations` holds one step per version, each rewriting the JSON tree from version n to n + 1. They run in `FileSaveStore` before the strict reader.
- Pros:
  - Every old save keeps loading.
  - It follows ADR 0007 as written.
- Cons:
  - Code and tests for each step, written only to carry the developer's test saves.
  - Each step must never change once written.

### B. Tolerant deserialization, with no version bump
- Pros: no migration code.
- Cons:
  - A damaged or mistyped file loads silently with defaults.
  - It contradicts ADR 0007's version rule.

### C. One versioned DTO per format, mapped to `SaveGame`
- Pros: each format is typed.
- Cons: heavy, and every change copies a DTO set.

### D. Until the first release, bump the version and refuse older saves
A format change bumps `formatVersion` and ships no migration. An older save is refused with a clear reason, as a newer one is today, and the developer deletes test saves.
- Pros:
  - No migration code for saves nobody keeps.
  - The reader stays strict, and the version still tells an old file from a damaged one.
- Cons:
  - Each format change loses the developer's test saves.
  - The migration mechanism (option A) has to be built at the first release.

## Decision
D (developer, 2026-10-24: "delete existing saves, I don't care, they are test saves"). It holds until the first release. From then on, ADR 0007's migration rule applies, with A as the expected mechanism. The developer's existing test save (`slot_01.json`) was deleted on 2026-10-24.

## Consequences
MP leaves in four steps, so each one builds and passes alone:

| Slice | What MP leaves | Notes |
|---|---|---|
| MP1 | Skills and effects | `MpCost` and `RestoreMp` go. |
| MP2 | Screens | Battle panel, menus, status page. |
| MP3a | Saves | Format 1 → 2: `PartyMemberProgress` loses `Mp`, and a format 1 save is refused. |
| MP3b | Members and enemies | `Vitals` and the class stats lose MP. |

- MP3a lands before `Vitals` loses MP, so a save never refers to a field the model lacks.
- **LR2** bumps the format to 3 the same way.
- **Old and newer saves:** `FileSaveStore` refuses an older format as "made by an older version of the game", alongside the existing refusals of a newer format and of a file with no version.
- **Tests:** every format change still has a round-trip test and a test that an older format is refused (CLAUDE.md).
- **Repair:** content repair (`SaveGameRepair`, decision 40) is unchanged.
- **At the first release:** write ADR 0007's first migration, then delete this rule from CLAUDE.md.
