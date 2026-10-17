# 0002. How content JSON maps to Core types

Status: accepted
Date: 2026-10-17

## Context
`content/` JSON is the source of truth for items, skills, party members, enemies and rules. `Jrpg.Data` loads and validates it; `Jrpg.Core` defines the definition records the game runs on (`PartyMemberDefinition`, `ItemDefinition`…); the editor (part 4) will save the same content back. Slice 3b builds the loading pipeline every later slice extends, so the mapping choice is made once, now.

Constraints: solo developer, so every extra type per concept is paid for in every slice. Core must stay free of I/O. Content must fail loudly at load time with the file, id and field named. Ids never change once used, and the file format should not change by accident.

## Options
### A. Deserialize straight into Core definition records
System.Text.Json reads `content/*.json` directly into the Core records, configured from Data (`ContentJsonOptions`: camelCase, string enums, required members, unknown members rejected). No serializer attributes in Core.
- Pros: one type per concept, no mapping code or mapping tests. The editor saves the same records it edits. Already how `PartyMemberDefinition` (`required init` properties) is shaped.
- Cons: the JSON shape is Core's public shape; renaming a Core property renames a JSON field. Records must stay serializer-friendly (settable `init` properties or a matching constructor), so invariants live in Data validators rather than constructors. Deserialized lists are mutable `List<T>` behind `IReadOnlyList<T>`.

### B. Separate DTOs in Data, mapped to Core types
Data has a `*Json` type per file that mirrors the JSON; a mapper builds the Core record.
- Pros: the file format and Core evolve independently. Core constructors can enforce invariants. Old formats can be read by a different DTO.
- Cons: two types plus a mapper per concept, and their field lists drift. Every slice that adds content pays for it. The editor needs the reverse mapping too.

### C. A by default, a DTO only where the file and the model genuinely differ
Use A for everything whose JSON matches the model. Write a Data-side DTO only for a format that doesn't fit: the legacy map/tile files (part 2) or polymorphic data like event steps if contract customization isn't enough.
- Pros: A's low cost for the common case, B's decoupling where it pays. The choice is made per file, with a reason.
- Cons: two patterns in Data; each DTO needs a one-line justification.

## Decision
**C** (developer, 2026-10-17). Load content straight into Core records; add a Data DTO only for a file whose format doesn't match the model, and say why in that loader.

## Consequences
- Core records stay attribute-free; all serializer configuration lives in `Jrpg.Data/Content/ContentJsonOptions`. If a polymorphic type needs discriminators, use contract customization in Data before adding attributes in Core.
- Renaming a Core definition property is a content format change: the "all of `content/` loads" test catches it, and the content must be updated in the same change.
- Range and cross-reference checks live in Data validators, not record constructors (e.g. the slice-4 `ExperienceTable` and party member checks).
- Runtime state (`PartyMember`, `Inventory`) is never serialized directly; save files get their own decision in part 2.
